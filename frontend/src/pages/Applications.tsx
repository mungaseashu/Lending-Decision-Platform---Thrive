import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../services/api';
import type { LoanApplicationResponse } from '../types';
import { Search, Filter, AlertCircle, RefreshCw, ChevronUp, ChevronDown } from 'lucide-react';

const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('en-GB', { style: 'currency', currency: 'GBP', maximumFractionDigits: 0 }).format(value);
};

const formatDate = (dateString: string) => {
    return new Intl.DateTimeFormat('en-GB', { 
        day: '2-digit', month: 'short', year: 'numeric', 
        hour: '2-digit', minute: '2-digit' 
    }).format(new Date(dateString));
};

type SortKey = 'createdAt' | 'loanAmount' | 'assetValue' | 'ltv' | 'creditScore' | 'status';

const Applications: React.FC = () => {
    const [applications, setApplications] = useState<LoanApplicationResponse[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [searchTerm, setSearchTerm] = useState('');
    const [statusFilter, setStatusFilter] = useState('All');
    const [sortKey, setSortKey] = useState<SortKey>('createdAt');
    const [sortAsc, setSortAsc] = useState(false);

    useEffect(() => {
        api.getApplications()
            .then(data => {
                setApplications(data);
            })
            .catch(err => setError(err.message || 'Failed to fetch applications.'))
            .finally(() => setLoading(false));
    }, []);

    const handleSort = (key: SortKey) => {
        if (sortKey === key) {
            setSortAsc(!sortAsc);
        } else {
            setSortKey(key);
            setSortAsc(false); // Default to desc for new sorts (highest/newest first)
        }
    };

    let processedApplications = applications.filter(app => {
        const matchesStatus = statusFilter === 'All' || app.decision.status === statusFilter;
        const searchString = searchTerm.toLowerCase();
        const matchesSearch = 
            app.id.toLowerCase().includes(searchString) ||
            app.loanAmount.toString().includes(searchString);
        return matchesStatus && matchesSearch;
    });

    processedApplications.sort((a, b) => {
        let aValue: any;
        let bValue: any;

        switch (sortKey) {
            case 'createdAt':
                aValue = new Date(a.createdAt).getTime();
                bValue = new Date(b.createdAt).getTime();
                break;
            case 'loanAmount':
                aValue = a.loanAmount;
                bValue = b.loanAmount;
                break;
            case 'assetValue':
                aValue = a.assetValue;
                bValue = b.assetValue;
                break;
            case 'ltv':
                aValue = a.ltv;
                bValue = b.ltv;
                break;
            case 'creditScore':
                aValue = a.creditScore;
                bValue = b.creditScore;
                break;
            case 'status':
                aValue = a.decision.status;
                bValue = b.decision.status;
                break;
        }

        if (aValue < bValue) return sortAsc ? -1 : 1;
        if (aValue > bValue) return sortAsc ? 1 : -1;
        return 0;
    });

    if (loading) {
        return (
            <div className="flex flex-col justify-center items-center h-64 text-blue-600">
                <RefreshCw className="h-8 w-8 animate-spin mb-4" />
                <p className="font-medium">Loading ledger data...</p>
            </div>
        );
    }

    if (error) {
        return (
            <div className="bg-red-50 border border-red-200 text-red-700 p-6 rounded-xl flex items-start shadow-sm max-w-4xl mx-auto">
                <AlertCircle className="h-6 w-6 mr-3 flex-shrink-0" />
                <div>
                    <h3 className="font-bold text-lg">Unable to load applications</h3>
                    <p className="mt-1">{error}</p>
                </div>
            </div>
        );
    }
    
    const SortIcon = ({ columnKey }: { columnKey: SortKey }) => {
        if (sortKey !== columnKey) return null;
        return sortAsc ? <ChevronUp className="inline h-4 w-4 ml-1" /> : <ChevronDown className="inline h-4 w-4 ml-1" />;
    };

    const Th = ({ columnKey, label }: { columnKey: SortKey, label: string }) => (
        <th 
            className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-50 transition-colors"
            onClick={() => handleSort(columnKey)}
        >
            <div className="flex items-center">
                {label}
                <SortIcon columnKey={columnKey} />
            </div>
        </th>
    );

    return (
        <div className="max-w-6xl mx-auto">
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center mb-6 gap-4">
                <div>
                    <h2 className="text-2xl font-bold text-gray-900">Application Ledger</h2>
                    <p className="text-gray-500 text-sm mt-1">View, filter, and sort historical loan decisions.</p>
                </div>
                <Link to="/new" className="bg-blue-600 text-white px-5 py-2.5 rounded-lg hover:bg-blue-700 font-semibold shadow-sm transition-colors whitespace-nowrap">
                    + New Application
                </Link>
            </div>

            <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
                <div className="p-4 border-b border-gray-100 flex flex-col sm:flex-row gap-4 bg-gray-50">
                    <div className="relative flex-1">
                        <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                            <Search className="h-4 w-4 text-gray-400" />
                        </div>
                        <input
                            type="text"
                            placeholder="Search by ID or amount..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                            className="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-lg focus:ring-1 focus:ring-blue-500 focus:border-blue-500 text-sm transition-colors outline-none"
                        />
                    </div>
                    <div className="relative w-full sm:w-48">
                        <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                            <Filter className="h-4 w-4 text-gray-400" />
                        </div>
                        <select
                            value={statusFilter}
                            onChange={(e) => setStatusFilter(e.target.value)}
                            className="block w-full pl-10 pr-10 py-2 text-sm border border-gray-300 rounded-lg focus:ring-1 focus:ring-blue-500 focus:border-blue-500 appearance-none bg-white transition-colors outline-none"
                        >
                            <option value="All">All Decisions</option>
                            <option value="Approved">Approved</option>
                            <option value="Declined">Declined</option>
                        </select>
                    </div>
                </div>

                {processedApplications.length === 0 ? (
                    <div className="p-12 text-center">
                        <div className="inline-flex items-center justify-center w-12 h-12 rounded-full bg-gray-100 mb-4">
                            <Search className="h-6 w-6 text-gray-400" />
                        </div>
                        <h3 className="text-lg font-medium text-gray-900">No applications found</h3>
                        <p className="mt-1 text-sm text-gray-500">
                            {applications.length === 0 
                                ? "No applications have been submitted to the platform yet." 
                                : "Try adjusting your search or filter criteria."}
                        </p>
                    </div>
                ) : (
                    <div className="overflow-x-auto">
                        <table className="min-w-full divide-y divide-gray-200">
                            <thead className="bg-white">
                                <tr>
                                    <Th columnKey="createdAt" label="Date Submitted" />
                                    <Th columnKey="loanAmount" label="Amount" />
                                    <Th columnKey="assetValue" label="Asset Value" />
                                    <Th columnKey="ltv" label="LTV" />
                                    <Th columnKey="creditScore" label="Score" />
                                    <Th columnKey="status" label="Decision" />
                                    <th className="px-6 py-4 text-right text-xs font-bold text-gray-500 uppercase tracking-wider">Action</th>
                                </tr>
                            </thead>
                            <tbody className="bg-white divide-y divide-gray-100">
                                {processedApplications.map((app) => (
                                    <tr key={app.id} className="hover:bg-gray-50 transition-colors">
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                            {formatDate(app.createdAt)}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm font-bold text-gray-900">
                                            {formatCurrency(app.loanAmount)}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-600">
                                            {formatCurrency(app.assetValue)}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-600">
                                            {app.ltv.toFixed(2)}%
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-600">
                                            {app.creditScore}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm">
                                            <span className={`px-2.5 py-1 inline-flex text-xs leading-5 font-bold rounded-full ${
                                                app.decision.status === 'Approved' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                                            }`}>
                                                {app.decision.status}
                                            </span>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                            <Link to={`/applications/${app.id}`} className="text-blue-600 hover:text-blue-900 font-semibold">View</Link>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>
        </div>
    );
};

export default Applications;
