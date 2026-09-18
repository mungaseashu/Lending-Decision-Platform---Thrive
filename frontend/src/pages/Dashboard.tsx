import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../services/api';
import type { DashboardStatsResponse, LoanApplicationResponse } from '../types';
import { Users, CheckCircle, XCircle, PoundSterling, Percent, ArrowRight, RefreshCw, AlertCircle } from 'lucide-react';

const Dashboard = () => {
    const [stats, setStats] = useState<DashboardStatsResponse | null>(null);
    const [recentApps, setRecentApps] = useState<LoanApplicationResponse[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        const fetchDashboardData = async () => {
            try {
                const [statsData, appsData] = await Promise.all([
                    api.getDashboardStats(),
                    api.getApplications()
                ]);
                
                setStats(statsData);
                
                // Sort applications by descending date and take top 5
                const sorted = appsData.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
                setRecentApps(sorted.slice(0, 5));
            } catch (err: any) {
                setError(err.message || 'Failed to load dashboard data.');
            } finally {
                setLoading(false);
            }
        };

        fetchDashboardData();
    }, []);

    const formatCurrency = (value: number) => new Intl.NumberFormat('en-GB', { style: 'currency', currency: 'GBP', minimumFractionDigits: 0 }).format(value);
    const formatDate = (dateStr: string) => new Intl.DateTimeFormat('en-GB', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(dateStr));

    if (loading) {
        return (
            <div className="flex flex-col items-center justify-center h-64 text-blue-600">
                <RefreshCw className="h-8 w-8 animate-spin mb-4" />
                <p className="font-medium">Loading authoritative data...</p>
            </div>
        );
    }

    if (error) {
        return (
            <div className="bg-red-50 border border-red-200 text-red-700 p-6 rounded-xl flex items-start shadow-sm">
                <AlertCircle className="h-6 w-6 mr-3 flex-shrink-0" />
                <div>
                    <h3 className="font-bold text-lg">Unable to load dashboard</h3>
                    <p className="mt-1">{error}</p>
                </div>
            </div>
        );
    }

    if (!stats) return null;

    const cards = [
        { name: 'Total Applicants', value: stats.totalApplicants, icon: Users, color: 'text-blue-600', bg: 'bg-blue-100' },
        { name: 'Successful', value: stats.successfulApplicants, icon: CheckCircle, color: 'text-green-600', bg: 'bg-green-100' },
        { name: 'Declined', value: stats.declinedApplicants, icon: XCircle, color: 'text-red-600', bg: 'bg-red-100' },
        { name: 'Total Value Written', value: formatCurrency(stats.totalValueOfLoansWritten), icon: PoundSterling, color: 'text-purple-600', bg: 'bg-purple-100' },
        { name: 'Mean Average LTV', value: `${stats.meanAverageLtv.toFixed(2)}%`, icon: Percent, color: 'text-yellow-600', bg: 'bg-yellow-100' }
    ];

    return (
        <div className="max-w-6xl mx-auto space-y-8">
            <div>
                <h2 className="text-2xl font-bold text-gray-900 mb-6">Overview</h2>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 gap-4">
                    {cards.map(card => {
                        const Icon = card.icon;
                        return (
                            <div key={card.name} className="bg-white rounded-xl shadow-sm p-5 border border-gray-100 flex flex-col justify-center">
                                <div className="flex items-center justify-between mb-3">
                                    <div className={`p-2.5 rounded-lg ${card.bg}`}>
                                        <Icon className={`h-5 w-5 ${card.color}`} />
                                    </div>
                                </div>
                                <div>
                                    <p className="text-2xl font-bold text-gray-900 tracking-tight">{card.value}</p>
                                    <p className="text-sm font-medium text-gray-500 mt-1 truncate">{card.name}</p>
                                </div>
                            </div>
                        );
                    })}
                </div>
            </div>

            <div>
                <div className="flex justify-between items-center mb-6">
                    <h2 className="text-2xl font-bold text-gray-900">Recent Applications</h2>
                    <Link to="/applications" className="text-sm font-medium text-blue-600 hover:text-blue-800 flex items-center">
                        View All <ArrowRight className="h-4 w-4 ml-1" />
                    </Link>
                </div>

                <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
                    {recentApps.length === 0 ? (
                        <div className="p-8 text-center text-gray-500">
                            No applications submitted yet.
                        </div>
                    ) : (
                        <div className="overflow-x-auto">
                            <table className="min-w-full divide-y divide-gray-200">
                                <thead className="bg-gray-50">
                                    <tr>
                                        <th className="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Date</th>
                                        <th className="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Amount</th>
                                        <th className="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">LTV</th>
                                        <th className="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Score</th>
                                        <th className="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase tracking-wider">Decision</th>
                                        <th className="px-6 py-3 text-right text-xs font-semibold text-gray-500 uppercase tracking-wider">Action</th>
                                    </tr>
                                </thead>
                                <tbody className="bg-white divide-y divide-gray-100">
                                    {recentApps.map((app) => (
                                        <tr key={app.id} className="hover:bg-gray-50 transition-colors">
                                            <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                                {formatDate(app.createdAt)}
                                            </td>
                                            <td className="px-6 py-4 whitespace-nowrap text-sm font-bold text-gray-900">
                                                {formatCurrency(app.loanAmount)}
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
                                                <Link to={`/applications/${app.id}`} className="text-blue-600 hover:text-blue-900">View</Link>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default Dashboard;
