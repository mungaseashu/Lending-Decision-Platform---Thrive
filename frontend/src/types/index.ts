export interface CreateLoanApplicationRequest {
    loanAmount: number;
    assetValue: number;
    creditScore: number;
}

export interface RuleResultDto {
    ruleName: string;
    isPassed: boolean;
    reason: string;
}

export interface LoanDecisionDto {
    status: string;
    calculatedLtv: number;
    ruleResults: RuleResultDto[];
    reasons: string[];
}

export interface LoanApplicationResponse {
    id: string;
    loanAmount: number;
    assetValue: number;
    creditScore: number;
    ltv: number;
    decision: LoanDecisionDto;
    createdAt: string;
}

export interface DashboardStatsResponse {
    totalApplicants: number;
    successfulApplicants: number;
    declinedApplicants: number;
    totalValueOfLoansWritten: number;
    meanAverageLtv: number;
}

export interface SimulationScenario {
    title: string;
    description: string;
    targetLoanAmount?: number;
    targetAssetValue?: number;
    targetCreditScore?: number;
}
