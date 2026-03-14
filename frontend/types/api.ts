export interface RepositoryDto {
  id: string;
  gitHubId: number;
  owner: string;
  name: string;
  fullName: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface PullRequestSummaryDto {
  id: string;
  gitHubPrNumber: number;
  title: string;
  headBranch: string;
  baseBranch: string;
  authorLogin: string;
  status: string;
  latestReviewId: string | null;
  latestTechDebtScore: number | null;
  latestReviewStatus: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface ReviewIssueDto {
  id: string;
  severity: string;
  category: string;
  filePath: string;
  lineNumber: number | null;
  description: string;
  suggestion: string;
}

export interface ReviewDetailDto {
  id: string;
  pullRequestId: string;
  gitHubPrNumber: number;
  pullRequestTitle: string;
  repositoryFullName: string;
  techDebtScore: number;
  summary: string;
  status: string;
  processingTimeMs: number | null;
  createdAt: string;
  issues: ReviewIssueDto[];
}

export interface DashboardSummaryDto {
  totalRepositories: number;
  totalPrsReviewed: number;
  averageTechDebtScore: number;
  criticalIssueCount: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
