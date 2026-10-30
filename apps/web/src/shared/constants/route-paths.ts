/** Toàn bộ đường dẫn web của app — luôn import từ đây, không viết chuỗi path trực tiếp trong component. */
export const ROUTE_PATHS = {
  login: '/dang-nhap',
  selectCompany: '/chon-cong-ty',
  dashboard: '/',
  admin: '/quan-tri',
  inventory: '/kho',
  accounting: '/ke-toan',
  projects: '/du-an'
} as const
