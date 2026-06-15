import { createBrowserRouter, Navigate, Outlet, useLocation } from "react-router";
import { Layout } from "./components/Layout";
import { useApp } from "./context/AppContext";

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
import PendingRejectedProperties from "./pages/landlord/PendingRejectedProperties";
import BillingPage from "./pages/landlord/BillingPage";
import ContractManagement from "./pages/landlord/ContractManagement";
import AppointmentManagement from "./pages/landlord/AppointmentManagement";
import UserManagement from "./pages/admin/UserManagement";
import ContentModeration from "./pages/admin/ContentModeration";
import SystemAnalytics from "./pages/admin/SystemAnalytics";
import ApprovedPostsManagement from "./pages/admin/ApprovedPostsManagement";
import ComplaintManagement from "./pages/admin/ComplaintManagement";
import RatingManagement from "./pages/admin/RatingManagement";
import AdminNotificationPage from "./pages/admin/AdminNotificationPage";
import NotificationsPage from "./pages/common/NotificationsPage";
import ProfilePage from "./pages/common/ProfilePage";
import Messages from "./pages/Messages";
import LandlordProfilePage from "./pages/common/LandlordProfilePage";

function RequireAuth() {
  const { isAuthenticated, authReady } = useApp();
  const location = useLocation();

  if (!authReady) {
    return null;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  return <Outlet />;
}

function RequireRole({ role }: { role: "tenant" | "landlord" | "admin" }) {
  const { currentUser, authReady } = useApp();

  if (!authReady) {
    return null;
  }

  if (!currentUser) {
    return <Navigate to="/login" replace />;
  }

  if (currentUser.role !== role) {
    return <Navigate to="/" replace />;
  }

  return <Outlet />;
}

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
      { index: true, Component: LandingPage },
      { path: "search", Component: SearchPage },
      { path: "rooms/:id", Component: RoomDetailPage },
      { path: "landlord-profile/:id", Component: LandlordProfilePage },
      {
        Component: RequireAuth,
        children: [
          {
            Component: () => <Outlet />,
            children: [
              { path: "notifications", Component: NotificationsPage },
              { path: "profile", Component: ProfilePage },
              { path: "messages", Component: Messages },
            ],
          },
          {
            Component: () => <RequireRole role="tenant" />,
            children: [
              { path: "tenant/dashboard", Component: TenantDashboard },
              { path: "tenant/contracts/:id", Component: ContractDetailPage },
              { path: "tenant/invoices/:id", Component: InvoiceDetailPage },
            ],
          },
          {
            Component: () => <RequireRole role="landlord" />,
            children: [
              { path: "landlord/dashboard", Component: LandlordDashboard },
              { path: "landlord/properties", Component: PropertyManagement },
              { path: "landlord/properties/pending-rejected", Component: PendingRejectedProperties },
              { path: "landlord/rooms/new", Component: RoomFormPage },
              { path: "landlord/rooms/:id/edit", Component: RoomFormPage },
              { path: "landlord/billing", Component: BillingPage },
              { path: "landlord/contracts", Component: ContractManagement },
              { path: "landlord/appointments", Component: AppointmentManagement },
            ],
          },
          {
            Component: () => <RequireRole role="admin" />,
            children: [
              { path: "admin/users", Component: UserManagement },
              { path: "admin/notifications/new", Component: AdminNotificationPage },
              { path: "admin/moderation", Component: ContentModeration },
              { path: "admin/analytics", Component: SystemAnalytics },
              { path: "admin/approved-posts", Component: ApprovedPostsManagement },
              { path: "admin/complaints", Component: ComplaintManagement },
              { path: "admin/ratings", Component: RatingManagement },
            ],
          },
        ],
      },
    ],
  },
]);
