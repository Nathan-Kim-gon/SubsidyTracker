export interface Subsidy {
  id: number;
  title: string;
  organization: string;
  amount: string | null;
  regionName: string;
  categoryName: string;
  targetGroups: string[];
  status: string;
  applicationEndDate: string | null;
  createdAt: string;
}

export interface SubsidyDetail extends Subsidy {
  description: string;
  eligibilityCriteria: string | null;
  applicationMethod: string | null;
  applicationUrl: string | null;
  sourceUrl: string | null;
  contactInfo: string | null;
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

export interface Region {
  id: number;
  name: string;
  code: string;
  children: Region[];
}

export interface Category {
  id: number;
  name: string;
  code: string;
  description: string;
}

export interface SubsidyFilter {
  keyword?: string;
  regionId?: number;
  categoryId?: number;
  page?: number;
  pageSize?: number;
}
