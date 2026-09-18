import { useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../services/api';
import type { CreateLoanApplicationRequest, LoanApplicationResponse } from '../types';
import { CheckCircle, XCircle, AlertCircle, RefreshCw, ArrowRight } from 'lucide-react';

const NewApplication = () => {
    const [loading, setLoading] = useState(false);
    const [apiError, setApiError] = useState('');
    const [result, setResult] = useState<LoanApplicationResponse | null>(null);

    const [formData, setFormData] = useState({
        loanAmount: '',
        assetValue: '',
        creditScore: ''
    });

    const [errors, setErrors] = useState({
        loanAmount: '',
        assetValue: '',
        creditScore: ''
    });

    const formatCurrency = (value: number) => new Intl.NumberFormat('en-GB', { style: 'currency', currency: 'GBP' }).format(value);

    const validate = () => {
        let valid = true;
        const newErrors = { loanAmount: '', assetValue: '', creditScore: '' };

        const loan = parseFloat(formData.loanAmount);
        const asset = parseFloat(formData.assetValue);
        const score = parseInt(formData.creditScore, 10);

        if (!formData.loanAmount || isNaN(loan) || loan <= 0) {
            newErrors.loanAmount = 'Loan amount must be greater than zero.';
            valid = false;
        }

        if (!formData.assetValue || isNaN(asset) || asset <= 0) {
            newErrors.assetValue = 'Asset value must be greater than zero.';
            valid = false;
        }

        if (!formData.creditScore || isNaN(score) || score < 1 || score > 999) {
            newErrors.creditScore = 'Credit score must be between 1 and 999.';
            valid = false;
        }

        setErrors(newErrors);
        return valid;
    };

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        // Only allow numbers
        if (value && !/^\d*\.?\d*$/.test(value)) return;
        setFormData({ ...formData, [name]: value });
        // Clear error when user types
        if (errors[name as keyof typeof errors]) {
            setErrors({ ...errors, [name]: '' });
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setApiError('');
        
        if (!validate()) return;

        setLoading(true);

        const request: CreateLoanApplicationRequest = {
            loanAmount: parseFloat(formData.loanAmount),
            assetValue: parseFloat(formData.assetValue),
            creditScore: parseInt(formData.creditScore, 10)
        };

        try {
            const res = await api.submitApplication(request);
            setResult(res);
        } catch (err: any) {
            setApiError(err.message || 'An unexpected error occurred.');
        } finally {
            setLoading(false);
        }
    };

    const resetForm = () => {
        setResult(null);
        setFormData({ loanAmount: '', assetValue: '', creditScore: '' });
        setErrors({ loanAmount: '', assetValue: '', creditScore: '' });
        setApiError('');
    };

    const loan = parseFloat(formData.loanAmount) || 0;
    const asset = parseFloat(formData.assetValue) || 0;
    const liveLtv = asset > 0 ? (loan / asset) * 100 : 0;

    if (result) {
        const isApproved = result.decision.status === 'Approved';
        
        return (
            <div className="max-w-3xl mx-auto space-y-6 animate-fade-in">
                <div className={`p-8 rounded-2xl shadow-sm border ${isApproved ? 'bg-green-50 border-green-200' : 'bg-red-50 border-red-200'}`}>
                    <div className="flex items-center flex-col text-center mb-8">
                        {isApproved ? (
                            <CheckCircle className="h-16 w-16 text-green-600 mb-4" />
                        ) : (
                            <XCircle className="h-16 w-16 text-red-600 mb-4" />
                        )}
                        <h2 className={`text-3xl font-bold ${isApproved ? 'text-green-900' : 'text-red-900'}`}>
                            Application {result.decision.status}
                        </h2>
                        <p className={`mt-2 ${isApproved ? 'text-green-700' : 'text-red-700'}`}>
                            {isApproved ? 'The loan meets all lending criteria and has been approved.' : 'The loan does not meet our lending criteria and has been declined.'}
                        </p>
                    </div>

                    <div className="bg-white rounded-xl p-6 shadow-sm mb-6 border border-gray-100">
                        <h3 className="text-lg font-semibold text-gray-900 mb-4 border-b pb-2">Application Summary</h3>
                        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                            <div>
                                <span className="block text-gray-500">Loan Amount</span>
                                <span className="font-semibold text-gray-900">{formatCurrency(result.loanAmount)}</span>
                            </div>
                            <div>
                                <span className="block text-gray-500">Asset Value</span>
                                <span className="font-semibold text-gray-900">{formatCurrency(result.assetValue)}</span>
                            </div>
                            <div>
                                <span className="block text-gray-500">Credit Score</span>
                                <span className="font-semibold text-gray-900">{result.creditScore}</span>
                            </div>
                            <div>
                                <span className="block text-gray-500">Calculated LTV</span>
                                <span className="font-semibold text-gray-900">{result.decision.calculatedLtv.toFixed(2)}%</span>
                            </div>
                        </div>
                    </div>

                    {!isApproved && result.decision.reasons.length > 0 && (
                        <div className="mb-6 p-5 bg-red-100 rounded-xl border border-red-200">
                            <h4 className="text-red-900 font-bold mb-3 flex items-center">
                                <AlertCircle className="h-5 w-5 mr-2" /> Decline Reasons
                            </h4>
                            <ul className="list-disc list-inside text-sm text-red-800 space-y-2">
                                {result.decision.reasons.map((reason, idx) => (
                                    <li key={idx}>{reason}</li>
                                ))}
                            </ul>
                        </div>
                    )}

                    <div className="bg-white rounded-xl p-6 shadow-sm border border-gray-100">
                        <h3 className="text-lg font-semibold text-gray-900 mb-4 border-b pb-2">Rule Evaluation Details</h3>
                        <div className="space-y-3">
                            {result.decision.ruleResults.map((rule, idx) => (
                                <div key={idx} className="flex items-start text-sm p-3 rounded-lg bg-gray-50 border border-gray-100">
                                    {rule.isPassed ? (
                                        <CheckCircle className="h-5 w-5 text-green-500 mr-3 flex-shrink-0 mt-0.5" />
                                    ) : (
                                        <XCircle className="h-5 w-5 text-red-500 mr-3 flex-shrink-0 mt-0.5" />
                                    )}
                                    <div>
                                        <p className="font-semibold text-gray-900">{rule.ruleName}</p>
                                        <p className={rule.isPassed ? "text-green-700 mt-1" : "text-red-600 mt-1"}>{rule.reason}</p>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>
                </div>

                <div className="flex justify-center space-x-4">
                    <button 
                        onClick={resetForm}
                        className="flex items-center px-6 py-3 bg-white border border-gray-300 rounded-xl text-gray-700 hover:bg-gray-50 font-medium transition-colors"
                    >
                        <RefreshCw className="h-4 w-4 mr-2" /> Submit Another
                    </button>
                    <Link 
                        to="/applications"
                        className="flex items-center px-6 py-3 bg-blue-600 rounded-xl text-white hover:bg-blue-700 font-medium transition-colors shadow-sm"
                    >
                        View All Applications <ArrowRight className="h-4 w-4 ml-2" />
                    </Link>
                </div>
            </div>
        );
    }

    return (
        <div className="max-w-2xl mx-auto">
            <div className="mb-8">
                <h2 className="text-3xl font-bold text-gray-900">New Loan Application</h2>
                <p className="text-gray-500 mt-2">Enter the applicant's financial details to receive an instant lending decision.</p>
            </div>
            
            <form onSubmit={handleSubmit} className="bg-white p-8 rounded-2xl shadow-sm border border-gray-100 space-y-6">
                {apiError && (
                    <div className="bg-red-50 text-red-700 p-4 rounded-xl text-sm flex items-start border border-red-100">
                        <AlertCircle className="h-5 w-5 mr-3 flex-shrink-0" />
                        <span>{apiError}</span>
                    </div>
                )}
                
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div>
                        <label htmlFor="loanAmount" className="block text-sm font-semibold text-gray-700 mb-1">Loan Amount</label>
                        <div className="relative">
                            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                                <span className="text-gray-500 font-medium">£</span>
                            </div>
                            <input 
                                id="loanAmount"
                                type="text" 
                                name="loanAmount"
                                maxLength={15}
                                value={formData.loanAmount}
                                onChange={handleChange}
                                placeholder="e.g. 500000"
                                className={`w-full pl-8 pr-4 py-3 border ${errors.loanAmount ? 'border-red-300 ring-1 ring-red-300' : 'border-gray-300 focus:border-blue-500 focus:ring-1 focus:ring-blue-500'} rounded-xl transition-all outline-none`}
                            />
                        </div>
                        {errors.loanAmount && <p className="text-red-500 text-xs mt-1 font-medium">{errors.loanAmount}</p>}
                    </div>
                    
                    <div>
                        <label htmlFor="assetValue" className="block text-sm font-semibold text-gray-700 mb-1">Asset Value</label>
                        <div className="relative">
                            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                                <span className="text-gray-500 font-medium">£</span>
                            </div>
                            <input 
                                id="assetValue"
                                type="text" 
                                name="assetValue"
                                maxLength={15}
                                value={formData.assetValue}
                                onChange={handleChange}
                                placeholder="e.g. 1000000"
                                className={`w-full pl-8 pr-4 py-3 border ${errors.assetValue ? 'border-red-300 ring-1 ring-red-300' : 'border-gray-300 focus:border-blue-500 focus:ring-1 focus:ring-blue-500'} rounded-xl transition-all outline-none`}
                            />
                        </div>
                        {errors.assetValue && <p className="text-red-500 text-xs mt-1 font-medium">{errors.assetValue}</p>}
                    </div>
                </div>
                
                <div>
                    <label htmlFor="creditScore" className="block text-sm font-semibold text-gray-700 mb-1">Credit Score</label>
                    <input 
                        id="creditScore"
                        type="text" 
                        name="creditScore"
                        maxLength={3}
                        value={formData.creditScore}
                        onChange={handleChange}
                        placeholder="1 - 999"
                        className={`w-full px-4 py-3 border ${errors.creditScore ? 'border-red-300 ring-1 ring-red-300' : 'border-gray-300 focus:border-blue-500 focus:ring-1 focus:ring-blue-500'} rounded-xl transition-all outline-none`}
                    />
                    {errors.creditScore && <p className="text-red-500 text-xs mt-1 font-medium">{errors.creditScore}</p>}
                </div>

                <div className="bg-blue-50 p-5 rounded-xl border border-blue-100 flex items-center justify-between">
                    <div>
                        <h4 className="text-blue-900 font-semibold text-sm">Live LTV Preview</h4>
                        <p className="text-blue-700 text-xs mt-1">Calculated for your convenience.</p>
                    </div>
                    <div className="text-2xl font-bold text-blue-700">
                        {liveLtv.toFixed(2)}%
                    </div>
                </div>

                <button 
                    type="submit" 
                    disabled={loading}
                    className="w-full bg-blue-600 text-white font-bold py-3 px-4 rounded-xl hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed transition-all shadow-sm"
                >
                    {loading ? (
                        <span className="flex items-center justify-center">
                            <RefreshCw className="animate-spin h-5 w-5 mr-2" />
                            Evaluating Application...
                        </span>
                    ) : (
                        'Submit for Instant Decision'
                    )}
                </button>
            </form>
        </div>
    );
};

export default NewApplication;
