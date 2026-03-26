import { createBrowserRouter } from "react-router";
import { Layout } from "./components/Layout";

// Pages
import LoginPage from "./pages/common/LoginPage";
import RegisterTenantPage from "./pages/common/RegisterTenantPage";
import RegisterLandlordPage from "./pages/common/RegisterLandlordPage";
import LandingPage from "./pages/tenant/LandingPage";
import SearchPage from "./pages/tenant/SearchPage";
import RoomDetailPage from "./pages/tenant/RoomDetailPage";
import TenantDashboard from "./pages/tenant/TenantDashboard";
import ContractDetailPage from "./pages/tenant/ContractDetailPage";
import InvoiceDetailPage from "./pages/tenant/InvoiceDetailPage";
import LandlordDashboard from "./pages/landlord/LandlordDashboard";
import PropertyManagement from "./pages/landlord/PropertyManagement";
import RoomFormPage from "./pages/landlord/RoomFormPage";
import ContractTemplatesPage from "./pages/landlord/ContractTemplatesPage";
import BillingPage from "./pages/landlord/BillingPage";
import ContractManagement from "./pages/landlord/ContractManagement";
import UserManagement from "./pages/admin/UserManagement";
import ContentModeration from "./pages/admin/ContentModeration";
import SystemAnalytics from "./pages/admin/SystemAnalytics";
import NotificationsPage from "./pages/common/NotificationsPage";
import ProfilePage from "./pages/common/ProfilePage";

export const router = createBrowserRouter([
  {
    path: "/login",
    Component: LoginPage,
  },
  {
    path: "/register/tenant",
    Component: RegisterTenantPage,
  },
  {
    path: "/register/landlord",
    Component: RegisterLandlordPage,
  },
  {
    path: "/",
    Component: Layout,
    children: [
      // Tenant Routes
      { index: true, Component: LandingPage },
      { path: "search", Component: SearchPage },
      { path: "rooms/:id", Component: RoomDetailPage },
      { path: "tenant/dashboard", Component: TenantDashboard },
      { path: "tenant/contracts/:id", Component: ContractDetailPage },
      { path: "tenant/invoices/:id", Component: InvoiceDetailPage },

      // Landlord Routes
      { path: "landlord/dashboard", Component: LandlordDashboard },
      { path: "landlord/properties", Component: PropertyManagement },
      { path: "landlord/rooms/new", Component: RoomFormPage },
      { path: "landlord/rooms/:id/edit", Component: RoomFormPage },
      { path: "landlord/contract-templates", Component: ContractTemplatesPage },
      { path: "landlord/billing", Component: BillingPage },
      { path: "landlord/contracts", Component: ContractManagement },

      // Admin Routes
      { path: "admin/users", Component: UserManagement },
      { path: "admin/moderation", Component: ContentModeration },
      { path: "admin/analytics", Component: SystemAnalytics },

      // Common Routes
      { path: "notifications", Component: NotificationsPage },
      { path: "profile", Component: ProfilePage },
    ],
  },
]);