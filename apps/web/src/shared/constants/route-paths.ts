/** Toàn bộ đường dẫn web của app — luôn import từ đây, không viết chuỗi path trực tiếp trong component. */
export const ROUTE_PATHS = {
  login: '/dang-nhap',
  selectCompany: '/chon-cong-ty',
  dashboard: '/',
  admin: '/quan-tri',
  adminCompanies: '/quan-tri/cong-ty',
  adminUsers: '/quan-tri/nguoi-dung',
  adminRoles: '/quan-tri/vai-tro',
  adminCatalogs: '/quan-tri/danh-muc',
  adminAuditLogs: '/quan-tri/nhat-ky',
  hrm: '/nhan-su',
  hrmDepartments: '/nhan-su/co-cau-to-chuc',
  inventory: '/kho',
  accounting: '/ke-toan',
  projects: '/du-an'
} as const
