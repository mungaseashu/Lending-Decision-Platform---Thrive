import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { api } from '../services/api';
import type { LoanApplicationResponse, SimulationScenario } from '../types';
import { ArrowLeft, CheckCircle, XCircle, Lightbulb } from 'lucide-react';

const ApplicationDetails = () => {
    const { id } = useParams<{ id: string }>();
    const [app, setApp] = useState<LoanApplicationResponse | null>(null);
    const [simulations, setSimulations] = useState<SimulationScenario[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        if (!id) return;
        
        const fetchData = async () => {
            try {
                const appData = await api.getApplication(id);
                setApp(appData);
                
                if (appData.decision.status !== 'Approved') {
                    const simData = await api.getSimulation(id);
                    setSimulations(simData);
                }
            } catch (err: any) {
                setError(err.message);
            } finally {
                setLoading(false);
            }
        };
        
        fetchData();
    }, [id]);

    const formatCurrency = (value: number) => new Intl.NumberFormat('en-GB', { style: 'currency', currency: 'GBP' }).format(value);

    if (loading) return <div className="text-gray-500">Loading details...</div>;
    if (error) return <div className="text-red-500 bg-red-50 p-4 rounded-md">{error}</div>;
    if (!app) return <div className="text-gray-500">Application not found.</div>;

    const isApproved = app.decision.status === 'Approved';

    return (
        <div className="max-w-4xl mx-auto">
            <div className="mb-6">
                <Link to="/applications" className="text-blue-600 hover:text-blue-800 flex items-center text-sm font-medium">
                    <ArrowLeft className="h-4 w-4 mr-1" /> Back to Applications
                </Link>
            </div>

            <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
                <div className={`px-6 py-4 border-b border-gray-100 flex items-center justify-between ${isApproved ? 'bg-green-50' : 'bg-red-50'}`}>
                    <div className="flex items-center">
                        {isApproved ? (
                            <CheckCircle className="h-6 w-6 text-green-600 mr-2" />
                        ) : (
                            <XCircle className="h-6 w-6 text-red-600 mr-2" />
                        )}
                        <h2 className={`text-xl font-bold ${isApproved ? 'text-green-900' : 'text-red-900'}`}>
                            Application {app.decision.status}
                        </h2>
                    </div>
                    <span className="text-sm text-gray-500">
                        {new Date(app.createdAt).toLocaleString()}
                    </span>
                </div>

                <div className="p-6 grid grid-cols-1 md:grid-cols-2 gap-8">
                    <div>
                        <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Application Details</h3>
                        <dl className="space-y-3 text-sm">
                            <div className="flex justify-between border-b border-gray-50 pb-2">
                                <dt className="text-gray-500">ID</dt>
                                <dd className="font-mono text-gray-900">{app.id}</dd>
                            </div>
                            <div className="flex justify-between border-b border-gray-50 pb-2">
                                <dt className="text-gray-500">Loan Amount</dt>
                                <dd className="font-medium text-gray-900">{formatCurrency(app.loanAmount)}</dd>
                            </div>
                            <div className="flex justify-between border-b border-gray-50 pb-2">
                                <dt className="text-gray-500">Asset Value</dt>
                                <dd className="font-medium text-gray-900">{formatCurrency(app.assetValue)}</dd>
                            </div>
                            <div className="flex justify-between border-b border-gray-50 pb-2">
                                <dt className="text-gray-500">LTV</dt>
                                <dd className="font-medium text-gray-900">{app.ltv.toFixed(2)}%</dd>
                            </div>
                            <div className="flex justify-between border-b border-gray-50 pb-2">
                                <dt className="text-gray-500">Credit Score</dt>
                                <dd className="font-medium text-gray-900">{app.creditScore}</dd>
                            </div>
                        </dl>
                    </div>

                    <div>
                        <h3 className="text-sm font-semibold text-gray-500 uppercase tracking-wider mb-4">Decision Breakdown</h3>
                        
                        {!isApproved && app.decision.reasons.length > 0 && (
                            <div className="mb-6 p-4 bg-red-50 rounded-md border border-red-100">
                                <h4 className="text-red-800 font-semibold mb-2">Decline Reasons:</h4>
                                <ul className="list-disc list-inside text-sm text-red-700 space-y-1">
                                    {app.decision.reasons.map((reason, idx) => (
                                        <li key={idx}>{reason}</li>
                                    ))}
                                </ul>
                            </div>
                        )}

                        <div>
                            <h4 className="text-gray-700 font-semibold mb-3">Rule Evaluations:</h4>
                            <div className="space-y-3">
                                {app.decision.ruleResults.map((rule, idx) => (
                                    <div key={idx} className="flex items-start text-sm bg-gray-50 p-3 rounded-md">
                                        {rule.isPassed ? (
                                            <CheckCircle className="h-5 w-5 text-green-500 mr-2 flex-shrink-0" />
                                        ) : (
                                            <XCircle className="h-5 w-5 text-red-500 mr-2 flex-shrink-0" />
                                        )}
                                        <div>
                                            <p className="font-medium text-gray-900">{rule.ruleName}</p>
                                            <p className={rule.isPassed ? "text-green-700 mt-1" : "text-red-600 mt-1"}>{rule.reason}</p>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        </div>
                    </div>
                </div>

                {!isApproved && simulations.length > 0 && (
                    <div className="bg-blue-50 border-t border-blue-100 p-6">
                        <div className="flex items-center mb-4">
                            <Lightbulb className="h-6 w-6 text-blue-600 mr-2" />
                            <h3 className="text-lg font-bold text-blue-900">Decision Simulator (Hypothetical)</h3>
                        </div>
                        <p className="text-sm text-blue-800 mb-6">
                            The following scenarios are mathematical calculations showing what parameters would be required to satisfy the failed rules. They do not guarantee future approval.
                        </p>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            {simulations.map((sim, idx) => (
                                <div key={idx} className="bg-white p-4 rounded-xl border border-blue-200 shadow-sm">
                                    <h4 className="font-bold text-gray-900 mb-1">{sim.title}</h4>
                                    <p className="text-sm text-gray-600 mb-4">{sim.description}</p>
                                    
                                    <div className="space-y-2">
                                        {sim.targetLoanAmount !== undefined && sim.targetLoanAmount !== null && (
                                            <div className="flex justify-between items-center text-sm bg-gray-50 p-2 rounded">
                                                <span className="text-gray-500">Required Loan Amount</span>
                                                <span className="font-semibold text-gray-900">{formatCurrency(sim.targetLoanAmount)}</span>
                                            </div>
                                        )}
                                        {sim.targetAssetValue !== undefined && sim.targetAssetValue !== null && (
                                            <div className="flex justify-between items-center text-sm bg-gray-50 p-2 rounded">
                                                <span className="text-gray-500">Required Asset Value</span>
                                                <span className="font-semibold text-gray-900">{formatCurrency(sim.targetAssetValue)}</span>
                                            </div>
                                        )}
                                        {sim.targetCreditScore !== undefined && sim.targetCreditScore !== null && (
                                            <div className="flex justify-between items-center text-sm bg-gray-50 p-2 rounded">
                                                <span className="text-gray-500">Required Credit Score</span>
                                                <span className="font-semibold text-gray-900">{sim.targetCreditScore}</span>
                                            </div>
                                        )}
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
};

export default ApplicationDetails;
