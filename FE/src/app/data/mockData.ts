export type Role = "tenant" | "landlord" | "admin";

export interface Room {
  id: string;
  title: string;
  address: string;
  district: string;
  price: number;
  area: number;
  maxPeople: number;
  status: "available" | "rented" | "maintenance";
  amenities: string[];
  images: string[];
  electricPrice: number;
  waterPrice: number;
  servicePrice: number;
  deposit: number;
  description: string;
  rating: number;
  reviews: Review[];
  floor: number;
  propertyId: string;
  lat: number;
  lng: number;
  postedDate: string;
}

export interface Review {
  id: string;
  author: string;
  avatar: string;
  rating: number;
  content: string;
  date: string;
}

export interface Property {
  id: string;
  name: string;
  address: string;
  district: string;
  totalRooms: number;
  floors: number;
  amenities: string[];
  status: "active" | "inactive";
}

export interface Invoice {
  id: string;
  roomId: string;
  roomTitle: string;
  tenantName: string;
  month: string;
  electricOld: number;
  electricNew: number;
  waterOld: number;
  waterNew: number;
  electricPrice: number;
  waterPrice: number;
  servicePrice: number;
  rentPrice: number;
  total: number;
  status: "paid" | "unpaid" | "overdue";
  dueDate: string;
  paidDate?: string;
}

export interface Contract {
  id: string;
  roomId: string;
  roomTitle: string;
  tenantName: string;
  tenantPhone: string;
  tenantIdCard: string;
  startDate: string;
  endDate: string;
  rentPrice: number;
  deposit: number;
  status: "active" | "expired" | "terminated";
}

export interface ViewingRequest {
  id: string;
  roomId: string;
  roomTitle: string;
  tenantName: string;
  tenantPhone: string;
  date: string;
  time: string;
  status: "pending" | "confirmed" | "rejected";
  note?: string;
}

export interface User {
  id: string;
  name: string;
  email: string;
  phone: string;
  role: Role;
  status: "active" | "locked";
  joinDate: string;
  avatar: string;
}

export interface Notification {
  id: string;
  title: string;
  message: string;
  type: "info" | "success" | "warning" | "error";
  read: boolean;
  date: string;
}

export interface PendingPost {
  id: string;
  title: string;
  landlordName: string;
  address: string;
  price: number;
  submittedDate: string;
  status: "pending" | "approved" | "rejected";
  images: string[];
  description: string;
  rejectionReason?: string;
}

// ============ MOCK DATA ============

export const mockRooms: Room[] = [
  {
    id: "r1",
    title: "Phòng trọ cao cấp có máy lạnh - Q. Bình Thạnh",
    address: "123 Đinh Tiên Hoàng, P. 1",
    district: "Bình Thạnh",
    price: 4500000,
    area: 25,
    maxPeople: 2,
    status: "available",
    amenities: ["Máy lạnh", "WC riêng", "Wifi", "Bảo vệ 24/7"],
    images: [
      "https://images.unsplash.com/photo-1737737196308-e5b848160b78?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=800",
      "https://images.unsplash.com/photo-1764836168197-3aa3a890a0f0?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=800",
      "https://images.unsplash.com/photo-1661796428175-55423b19409f?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=800",
    ],
    electricPrice: 3500,
    waterPrice: 15000,
    servicePrice: 200000,
    deposit: 9000000,
    description:
      "Phòng trọ cao cấp nằm ngay trung tâm quận Bình Thạnh. Phòng rộng rãi, thoáng mát, có đầy đủ tiện nghi. Gần chợ, trường học, bệnh viện. An ninh tốt, có bảo vệ 24/7.",
    rating: 4.5,
    reviews: [
      {
        id: "rev1",
        author: "Nguyễn Văn A",
        avatar: "https://i.pravatar.cc/40?img=1",
        rating: 5,
        content: "Phòng rất sạch sẽ, chủ trọ tốt bụng. Rất hài lòng!",
        date: "2025-11-15",
      },
      {
        id: "rev2",
        author: "Trần Thị B",
        avatar: "https://i.pravatar.cc/40?img=2",
        rating: 4,
        content: "Vị trí thuận tiện, giá hợp lý. Chỉ tiếc không có chỗ để xe rộng.",
        date: "2025-10-20",
      },
    ],
    floor: 2,
    propertyId: "p1",
    lat: 10.807,
    lng: 106.716,
    postedDate: "2025-12-01",
  },
  {
    id: "r2",
    title: "Phòng có gác lửng - Gần ĐH Bách Khoa",
    address: "45 Tô Hiến Thành, P. 15",
    district: "Quận 10",
    price: 3800000,
    area: 30,
    maxPeople: 2,
    status: "available",
    amenities: ["Có gác lửng", "Máy lạnh", "Wifi", "Thú cưng được"],
    images: [
      "https://images.unsplash.com/photo-1764836168197-3aa3a890a0f0?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=800",
      "https://images.unsplash.com/photo-1737737196308-e5b848160b78?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=800",
    ],
    electricPrice: 3000,
    waterPrice: 12000,
    servicePrice: 150000,
    deposit: 7600000,
    description:
      "Phòng có gác lửng rộng 30m2, gần Đại học Bách Khoa. Cho phép nuôi thú cưng. Khu vực yên tĩnh, phù hợp sinh viên và người đi làm.",
    rating: 4.2,
    reviews: [
      {
        id: "rev3",
        author: "Lê Minh C",
        avatar: "https://i.pravatar.cc/40?img=3",
        rating: 4,
        content: "Phòng gác lửng rất tiện. Chủ trọ friendly, cho nuôi mèo thoải mái!",
        date: "2025-11-01",
      },
    ],
    floor: 1,
    propertyId: "p1",
    lat: 10.773,
    lng: 106.658,
    postedDate: "2025-12-05",
  },
  {
    id: "r3",
    title: "Studio cao cấp full nội thất - Quận 7",
    address: "88 Nguyễn Thị Thập, P. Tân Phú",
    district: "Quận 7",
    price: 6500000,
    area: 35,
    maxPeople: 2,
    status: "available",
    amenities: ["Máy lạnh", "WC riêng", "Full nội thất", "Wifi", "Hầm xe"],
    images: [
      "https://images.unsplash.com/photo-1661796428175-55423b19409f?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=800",
    ],
    electricPrice: 3500,
    waterPrice: 15000,
    servicePrice: 300000,
    deposit: 13000000,
    description:
      "Studio cao cấp full nội thất tại Quận 7. Thiết kế hiện đại, view thoáng, đầy đủ tiện nghi từ tủ lạnh, máy giặt đến nệm cao su.",
    rating: 4.8,
    reviews: [],
    floor: 5,
    propertyId: "p2",
    lat: 10.737,
    lng: 106.717,
    postedDate: "2025-11-28",
  },
  {
    id: "r4",
    title: "Phòng trọ giá rẻ - Gò Vấp",
    address: "22 Phan Văn Trị, P. 11",
    district: "Gò Vấp",
    price: 2800000,
    area: 20,
    maxPeople: 2,
    status: "rented",
    amenities: ["WC riêng", "Wifi"],
    images: [
      "https://images.unsplash.com/photo-1771337744364-e7dd00c2921c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=800",
    ],
    electricPrice: 2800,
    waterPrice: 10000,
    servicePrice: 100000,
    deposit: 5600000,
    description: "Phòng trọ giá rẻ khu vực Gò Vấp. Phù hợp sinh viên hoặc người mới đi làm.",
    rating: 3.8,
    reviews: [],
    floor: 1,
    propertyId: "p2",
    lat: 10.842,
    lng: 106.668,
    postedDate: "2025-11-10",
  },
  {
    id: "r5",
    title: "Phòng cho nuôi thú cưng - Thủ Đức",
    address: "66 Kha Vạn Cân, P. Linh Đông",
    district: "Thủ Đức",
    price: 3200000,
    area: 22,
    maxPeople: 2,
    status: "available",
    amenities: ["Máy lạnh", "Thú cưng được", "Wifi", "WC riêng"],
    images: [
      "https://images.unsplash.com/photo-1602646994030-464f98de5e5c?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=800",
    ],
    electricPrice: 3000,
    waterPrice: 12000,
    servicePrice: 150000,
    deposit: 6400000,
    description: "Phòng thoáng mát, cho phép nuôi thú cưng. Gần trung tâm Thủ Đức, tiện đi lại.",
    rating: 4.0,
    reviews: [],
    floor: 3,
    propertyId: "p3",
    lat: 10.858,
    lng: 106.756,
    postedDate: "2025-12-10",
  },
  {
    id: "r6",
    title: "Phòng trọ yên tĩnh - Quận 3",
    address: "15 Trần Quốc Thảo, P. 6",
    district: "Quận 3",
    price: 5200000,
    area: 28,
    maxPeople: 2,
    status: "available",
    amenities: ["Máy lạnh", "WC riêng", "Wifi", "Bảo vệ 24/7", "Thang máy"],
    images: [
      "https://images.unsplash.com/photo-1737737196308-e5b848160b78?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=800",
    ],
    electricPrice: 3500,
    waterPrice: 15000,
    servicePrice: 250000,
    deposit: 10400000,
    description: "Phòng trọ cao cấp tại Quận 3 trung tâm. Yên tĩnh, an ninh tốt, có thang máy.",
    rating: 4.6,
    reviews: [],
    floor: 4,
    propertyId: "p3",
    lat: 10.779,
    lng: 106.691,
    postedDate: "2025-12-08",
  },
];

export const mockProperties: Property[] = [
  {
    id: "p1",
    name: "Khu trọ Bình Thạnh A",
    address: "123 Đinh Tiên Hoàng, P. 1, Quận Bình Thạnh",
    district: "Bình Thạnh",
    totalRooms: 12,
    floors: 4,
    amenities: ["Bảo vệ 24/7", "Wifi chung", "Hầm xe"],
    status: "active",
  },
  {
    id: "p2",
    name: "Khu trọ Quận 7 Premium",
    address: "88 Nguyễn Thị Thập, P. Tân Phú, Quận 7",
    district: "Quận 7",
    totalRooms: 8,
    floors: 6,
    amenities: ["Thang máy", "Hầm xe", "Camera an ninh"],
    status: "active",
  },
  {
    id: "p3",
    name: "Nhà trọ Thủ Đức",
    address: "66 Kha Vạn Cân, P. Linh Đông, Thủ Đức",
    district: "Thủ Đức",
    totalRooms: 6,
    floors: 3,
    amenities: ["Wifi chung", "Sân phơi"],
    status: "active",
  },
];

export const mockInvoices: Invoice[] = [
  {
    id: "inv1",
    roomId: "r1",
    roomTitle: "Phòng 101 - Khu trọ Bình Thạnh A",
    tenantName: "Nguyễn Văn A",
    month: "12/2025",
    electricOld: 1240,
    electricNew: 1380,
    waterOld: 24,
    waterNew: 28,
    electricPrice: 3500,
    waterPrice: 15000,
    servicePrice: 200000,
    rentPrice: 4500000,
    total: 5350000,
    status: "unpaid",
    dueDate: "2026-01-05",
  },
  {
    id: "inv2",
    roomId: "r2",
    roomTitle: "Phòng 201 - Khu trọ Bình Thạnh A",
    tenantName: "Trần Thị B",
    month: "12/2025",
    electricOld: 890,
    electricNew: 1020,
    waterOld: 18,
    waterNew: 22,
    electricPrice: 3000,
    waterPrice: 12000,
    servicePrice: 150000,
    rentPrice: 3800000,
    total: 4538000,
    status: "paid",
    dueDate: "2026-01-05",
    paidDate: "2025-12-28",
  },
  {
    id: "inv3",
    roomId: "r3",
    roomTitle: "Phòng 501 - Khu trọ Quận 7",
    tenantName: "Lê Minh C",
    month: "11/2025",
    electricOld: 650,
    electricNew: 790,
    waterOld: 15,
    waterNew: 19,
    electricPrice: 3500,
    waterPrice: 15000,
    servicePrice: 300000,
    rentPrice: 6500000,
    total: 7449000,
    status: "overdue",
    dueDate: "2025-12-05",
  },
  {
    id: "inv4",
    roomId: "r4",
    roomTitle: "Phòng 102 - Khu trọ Bình Thạnh A",
    tenantName: "Phạm Thị D",
    month: "12/2025",
    electricOld: 1100,
    electricNew: 1230,
    waterOld: 20,
    waterNew: 24,
    electricPrice: 3500,
    waterPrice: 15000,
    servicePrice: 200000,
    rentPrice: 4500000,
    total: 5315000,
    status: "unpaid",
    dueDate: "2026-01-05",
  },
  {
    id: "inv5",
    roomId: "r5",
    roomTitle: "Phòng 301 - Nhà trọ Thủ Đức",
    tenantName: "Hoàng Văn E",
    month: "12/2025",
    electricOld: 780,
    electricNew: 890,
    waterOld: 16,
    waterNew: 20,
    electricPrice: 3000,
    waterPrice: 12000,
    servicePrice: 150000,
    rentPrice: 3200000,
    total: 3828000,
    status: "paid",
    dueDate: "2026-01-05",
    paidDate: "2025-12-30",
  },
];

export const mockContracts: Contract[] = [
  {
    id: "c1",
    roomId: "r1",
    roomTitle: "Phòng 101 - Khu trọ Bình Thạnh A",
    tenantName: "Nguyễn Văn A",
    tenantPhone: "0901234567",
    tenantIdCard: "079123456789",
    startDate: "2025-03-01",
    endDate: "2026-03-01",
    rentPrice: 4500000,
    deposit: 9000000,
    status: "active",
  },
  {
    id: "c2",
    roomId: "r2",
    roomTitle: "Phòng 201 - Khu trọ Bình Thạnh A",
    tenantName: "Trần Thị B",
    tenantPhone: "0912345678",
    tenantIdCard: "079987654321",
    startDate: "2025-06-01",
    endDate: "2026-06-01",
    rentPrice: 3800000,
    deposit: 7600000,
    status: "active",
  },
  {
    id: "c3",
    roomId: "r3",
    roomTitle: "Phòng 501 - Khu trọ Quận 7",
    tenantName: "Lê Minh C",
    tenantPhone: "0923456789",
    tenantIdCard: "079111222333",
    startDate: "2024-12-01",
    endDate: "2025-12-01",
    rentPrice: 6500000,
    deposit: 13000000,
    status: "expired",
  },
  {
    id: "c4",
    roomId: "r5",
    roomTitle: "Phòng 301 - Nhà trọ Thủ Đức",
    tenantName: "Hoàng Văn E",
    tenantPhone: "0934567890",
    tenantIdCard: "079444555666",
    startDate: "2025-09-01",
    endDate: "2026-09-01",
    rentPrice: 3200000,
    deposit: 6400000,
    status: "active",
  },
];

export const mockViewingRequests: ViewingRequest[] = [
  {
    id: "vr1",
    roomId: "r1",
    roomTitle: "Phòng trọ cao cấp có máy lạnh - Q. Bình Thạnh",
    tenantName: "Vũ Thị F",
    tenantPhone: "0945678901",
    date: "2026-01-10",
    time: "14:00",
    status: "confirmed",
    note: "Tôi muốn xem phòng vào buổi chiều",
  },
  {
    id: "vr2",
    roomId: "r2",
    roomTitle: "Phòng có gác lửng - Gần ĐH Bách Khoa",
    tenantName: "Đặng Văn G",
    tenantPhone: "0956789012",
    date: "2026-01-11",
    time: "10:00",
    status: "pending",
  },
  {
    id: "vr3",
    roomId: "r6",
    roomTitle: "Phòng trọ yên tĩnh - Quận 3",
    tenantName: "Bùi Thị H",
    tenantPhone: "0967890123",
    date: "2026-01-08",
    time: "09:00",
    status: "rejected",
    note: "Phòng đã được đặt cọc bởi người khác",
  },
];

export const mockUsers: User[] = [
  {
    id: "u1",
    name: "Nguyễn Văn An",
    email: "nguyenvanan@email.com",
    phone: "0901234567",
    role: "tenant",
    status: "active",
    joinDate: "2025-03-15",
    avatar: "https://i.pravatar.cc/40?img=11",
  },
  {
    id: "u2",
    name: "Trần Thị Bình",
    email: "tranthibinh@email.com",
    phone: "0912345678",
    role: "tenant",
    status: "active",
    joinDate: "2025-06-20",
    avatar: "https://i.pravatar.cc/40?img=12",
  },
  {
    id: "u3",
    name: "Lê Văn Chính",
    email: "levanchinh@email.com",
    phone: "0923456789",
    role: "landlord",
    status: "active",
    joinDate: "2024-11-01",
    avatar: "https://i.pravatar.cc/40?img=13",
  },
  {
    id: "u4",
    name: "Phạm Thị Dung",
    email: "phamthidung@email.com",
    phone: "0934567890",
    role: "landlord",
    status: "active",
    joinDate: "2025-01-10",
    avatar: "https://i.pravatar.cc/40?img=14",
  },
  {
    id: "u5",
    name: "Hoàng Văn Em",
    email: "hoangvanem@email.com",
    phone: "0945678901",
    role: "tenant",
    status: "locked",
    joinDate: "2025-08-05",
    avatar: "https://i.pravatar.cc/40?img=15",
  },
  {
    id: "u6",
    name: "Vũ Thị Fương",
    email: "vuthifuong@email.com",
    phone: "0956789012",
    role: "tenant",
    status: "active",
    joinDate: "2025-09-12",
    avatar: "https://i.pravatar.cc/40?img=16",
  },
  {
    id: "u7",
    name: "Đặng Văn Giang",
    email: "dangvangiang@email.com",
    phone: "0967890123",
    role: "tenant",
    status: "active",
    joinDate: "2025-10-30",
    avatar: "https://i.pravatar.cc/40?img=17",
  },
  {
    id: "u8",
    name: "Bùi Thị Hoa",
    email: "buithihoa@email.com",
    phone: "0978901234",
    role: "landlord",
    status: "locked",
    joinDate: "2025-02-14",
    avatar: "https://i.pravatar.cc/40?img=18",
  },
];

export const mockPendingPosts: PendingPost[] = [
  {
    id: "pp1",
    title: "Phòng trọ mới mở - Quận 12",
    landlordName: "Nguyễn Hữu Tài",
    address: "123 Tô Ký, P. Trung Mỹ Tây, Quận 12",
    price: 3500000,
    submittedDate: "2026-01-08",
    status: "pending",
    images: [
      "https://images.unsplash.com/photo-1764836168197-3aa3a890a0f0?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=400",
    ],
    description: "Phòng mới xây, sạch sẽ, an ninh. Gần chợ Trung Mỹ Tây.",
  },
  {
    id: "pp2",
    title: "Studio mini Bình Dương giá rẻ",
    landlordName: "Trần Thị Lan",
    address: "45 Đại lộ Bình Dương, P. Phú Hòa, Bình Dương",
    price: 2500000,
    submittedDate: "2026-01-07",
    status: "pending",
    images: [
      "https://images.unsplash.com/photo-1661796428175-55423b19409f?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=400",
    ],
    description: "Studio mini phù hợp công nhân, người đi làm. Sát KCN.",
  },
  {
    id: "pp3",
    title: "Phòng trọ sai địa chỉ test",
    landlordName: "Tài khoản lạ",
    address: "999 Đường Không Tồn Tại",
    price: 100000,
    submittedDate: "2026-01-06",
    status: "rejected",
    images: [],
    description: "Nội dung không hợp lệ",
    rejectionReason: "Địa chỉ không tồn tại, giá không hợp lý, nội dung vi phạm quy định.",
  },
  {
    id: "pp4",
    title: "Căn hộ cao cấp Vinhomes Grand Park",
    landlordName: "Phạm Minh Khoa",
    address: "Vinhomes Grand Park, Long Bình, Quận 9",
    price: 8000000,
    submittedDate: "2026-01-05",
    status: "approved",
    images: [
      "https://images.unsplash.com/photo-1737737196308-e5b848160b78?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&w=400",
    ],
    description: "Căn hộ 1 phòng ngủ cao cấp trong khu đô thị Vinhomes Grand Park.",
  },
];

export const mockNotifications: Notification[] = [
  {
    id: "n1",
    title: "Hóa đơn tháng 12/2025",
    message: "Hóa đơn tiền phòng tháng 12/2025 đã được tạo. Tổng cộng: 5.350.000đ. Hạn thanh toán: 05/01/2026.",
    type: "info",
    read: false,
    date: "2026-01-01",
  },
  {
    id: "n2",
    title: "Xác nhận lịch xem phòng",
    message: "Lịch xem phòng của bạn vào ngày 10/01/2026 lúc 14:00 đã được xác nhận. Chủ trọ sẽ đón bạn.",
    type: "success",
    read: false,
    date: "2025-12-28",
  },
  {
    id: "n3",
    title: "Nhắc nhở thanh toán",
    message: "Bạn còn 5 ngày để thanh toán hóa đơn tháng 12/2025. Vui lòng thanh toán đúng hạn.",
    type: "warning",
    read: true,
    date: "2025-12-31",
  },
  {
    id: "n4",
    title: "Hợp đồng sắp hết hạn",
    message: "Hợp đồng thuê phòng của bạn sẽ hết hạn vào ngày 01/03/2026. Vui lòng liên hệ chủ trọ để gia hạn.",
    type: "warning",
    read: true,
    date: "2025-12-25",
  },
  {
    id: "n5",
    title: "Cập nhật hệ thống",
    message: "Hệ thống sẽ bảo trì vào ngày 15/01/2026 từ 00:00 đến 02:00. Xin lỗi vì bất tiện.",
    type: "info",
    read: true,
    date: "2025-12-20",
  },
];

export const revenueChartData = [
  { month: "T7", revenue: 42000000 },
  { month: "T8", revenue: 47500000 },
  { month: "T9", revenue: 45000000 },
  { month: "T10", revenue: 52000000 },
  { month: "T11", revenue: 49000000 },
  { month: "T12", revenue: 55800000 },
];

export const systemStats = {
  totalUsers: 1248,
  newUsersThisMonth: 87,
  totalRooms: 342,
  activeRooms: 289,
  totalTransactions: 1876,
  successfulTransactions: 1743,
  totalRevenue: 2840000000,
};

export const formatCurrency = (amount: number): string => {
  return new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
  }).format(amount);
};
