import type { CreateLoanApplicationRequest, LoanApplicationResponse, DashboardStatsResponse } from '../types';

const API_BASE = 'http://localhost:5062/api';

export const api = {
    async getDashboardStats(): Promise<DashboardStatsResponse> {
        const res = await fetch(`${API_BASE}/dashboard`);
        if (!res.ok) throw new Error('Failed to fetch dashboard stats');
        return res.json();
    },

    async getApplications(): Promise<LoanApplicationResponse[]> {
        const res = await fetch(`${API_BASE}/loan-applications`);
        if (!res.ok) throw new Error('Failed to fetch applications');
        return res.json();
    },

    async getApplication(id: string): Promise<LoanApplicationResponse> {
        const res = await fetch(`${API_BASE}/loan-applications/${id}`);
        if (!res.ok) throw new Error('Failed to fetch application details');
        return res.json();
    },

    async getSimulation(id: string): Promise<any[]> {
        const res = await fetch(`${API_BASE}/loan-applications/${id}/simulate`);
        if (!res.ok) throw new Error('Failed to fetch simulation');
        return res.json();
    },

    async submitApplication(data: CreateLoanApplicationRequest): Promise<LoanApplicationResponse> {
        const res = await fetch(`${API_BASE}/loan-applications`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        
        if (!res.ok) {
            let errorMessage = 'Failed to submit application';
            try {
                const errData = await res.json();
                errorMessage = errData.detail || errData.title || errorMessage;
            } catch (e) {
                // Ignore parse error
            }
            throw new Error(errorMessage);
        }
        return res.json();
    }
};
