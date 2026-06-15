import { useEffect, useState } from "react";
import { X, Calendar, Star, AlertTriangle, Home, FileText, User } from "lucide-react";
import type { UserResponse } from "../../lib/types";
import { normalizeRole } from "../../lib/auth";
import { getComplaints } from "../../lib/complaints";
import { getRatings } from "../../lib/ratings";
import { getAppointments } from "../../lib/appointments";
import { getProperties, getPropertyById } from "../../lib/properties";
import { Link } from "react-router";
import { useApp } from "../../context/AppContext";

type Tab = "profile" | "properties" | "appointments" | "complaints" | "ratings";

export default function UserDetailDrawer({ user, onClose }: { user: UserResponse; onClose: () => void }) {
  const { token } = useApp();
  const role = normalizeRole(user.role);
  
  const [activeTab, setActiveTab] = useState<Tab>("profile");
  
  // Data states
  const [properties, setProperties] = useState<any[]>([]);
  const [appointments, setAppointments] = useState<any[]>([]);
  const [complaints, setComplaints] = useState<any[]>([]);
  const [ratings, setRatings] = useState<any[]>([]);
  const [propertyNames, setPropertyNames] = useState<Record<string, string>>({});
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    let cancelled = false;
    
    async function loadData() {
      if (!token) return;
      setLoading(true);
      try {
        let currentProperties: any[] = [];
        let currentAppointments: any[] = [];
        let currentRatings: any[] = [];

        if (activeTab === "properties" && role === "landlord") {
          const res = await getProperties({ landlordId: user.id, pageSize: 100 }, token);
          currentProperties = res.items || [];
          if (!cancelled) setProperties(currentProperties);
        } else if (activeTab === "appointments") {
          const queryParams: any = { pageSize: 100 };
          if (role === "tenant") queryParams.userId = user.id;
          else if (role === "landlord") queryParams.landlordId = user.id;
          
          const res = await getAppointments(token, queryParams);
          currentAppointments = res.items || [];
          if (!cancelled) setAppointments(currentAppointments);
        } else if (activeTab === "complaints") {
          const queryParams: any = { pageSize: 100 };
          if (role === "tenant") queryParams.creatorId = user.id;
          else if (role === "landlord") queryParams.landlordId = user.id;
          
          const res = await getComplaints(queryParams, token);
          const filtered = res.items || [];
          if (!cancelled) setComplaints(filtered);
        } else if (activeTab === "ratings") {
          const queryParams: any = { pageSize: 100 };
          if (role === "tenant") queryParams.tenantId = user.id;
          else if (role === "landlord") queryParams.landlordId = user.id;
          
          const res = await getRatings(queryParams, token);
          currentRatings = res.items || [];
          if (!cancelled) setRatings(currentRatings);
        }

        // Fetch missing property names
        const uniqueIds = new Set<string>();
        currentProperties.forEach(p => { uniqueIds.add(p.id); });
        currentAppointments.forEach(a => uniqueIds.add(a.propertyId));
        currentRatings.forEach(r => uniqueIds.add(r.propertyId));

        if (uniqueIds.size > 0 && !cancelled) {
          const missingIds = Array.from(uniqueIds).filter(id => !propertyNames[id]);
          if (missingIds.length > 0) {
            const promises = missingIds.map(id => getPropertyById(id).catch(() => null));
            const results = await Promise.all(promises);
            if (!cancelled) {
              setPropertyNames(prev => {
                const next = { ...prev };
                currentProperties.forEach(p => { next[p.id] = p.propertyName; });
                results.forEach((res, idx) => {
                  if (res) next[missingIds[idx]] = res.propertyName;
                });
                return next;
              });
            }
          }
        }
      } catch (err) {
        console.error("Failed to load tab data", err);
      } finally {
        if (!cancelled) setLoading(false);
      }
    }
    
    if (activeTab !== "profile") {
      loadData();
    }
    
    return () => { cancelled = true; };
  }, [activeTab, user.id, token, role]);

  return (
    <>
      <div className="fixed inset-0 bg-black/30 z-40 transition-opacity" onClick={onClose} />
      <div className="fixed top-0 right-0 bottom-0 w-full max-w-xl bg-white shadow-2xl z-50 flex flex-col transform transition-transform duration-300">
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
          <h2 className="text-lg font-bold text-gray-900">Chi tiết người dùng</h2>
          <button onClick={onClose} className="p-2 text-gray-400 hover:bg-gray-100 rounded-full transition-colors">
            <X className="w-5 h-5" />
          </button>
        </div>
        
        <div className="flex border-b border-gray-100 px-2 overflow-x-auto">
          <TabButton active={activeTab === "profile"} onClick={() => setActiveTab("profile")} icon={<User className="w-4 h-4" />} label="Hồ sơ" />
          {role === "landlord" && (
            <TabButton active={activeTab === "properties"} onClick={() => setActiveTab("properties")} icon={<Home className="w-4 h-4" />} label="Phòng trọ" />
          )}
          <TabButton active={activeTab === "appointments"} onClick={() => setActiveTab("appointments")} icon={<Calendar className="w-4 h-4" />} label="Lịch hẹn" />
          <TabButton active={activeTab === "complaints"} onClick={() => setActiveTab("complaints")} icon={<AlertTriangle className="w-4 h-4" />} label="Khiếu nại" />
          <TabButton active={activeTab === "ratings"} onClick={() => setActiveTab("ratings")} icon={<Star className="w-4 h-4" />} label="Đánh giá" />
        </div>
        
        <div className="flex-1 overflow-y-auto p-6 bg-gray-50">
          {activeTab === "profile" && (
            <div className="bg-white rounded-2xl p-6 border border-gray-100 shadow-sm space-y-6">
              <div className="flex items-center gap-4">
                <img
                  src={user.avatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(user.fullName)}&background=f97316&color=fff`}
                  alt=""
                  className="w-20 h-20 rounded-full object-cover border-4 border-white shadow-sm"
                />
                <div>
                  <h3 className="text-xl font-bold text-gray-900">{user.fullName}</h3>
                  <p className="text-gray-500 text-sm">ID: {user.id}</p>
                  <div className="mt-2 flex gap-2">
                    <span className={`px-2.5 py-1 rounded-xl text-xs font-semibold ${role === "landlord" ? "bg-orange-100 text-orange-600" : "bg-blue-100 text-blue-600"}`}>
                      {role === "landlord" ? "Chủ trọ" : "Khách thuê"}
                    </span>
                    <span className={`px-2.5 py-1 rounded-xl text-xs font-semibold ${user.isBlocked ? "bg-red-100 text-red-600" : "bg-green-100 text-green-600"}`}>
                      {user.isBlocked ? "Đã khóa" : "Hoạt động"}
                    </span>
                  </div>
                </div>
              </div>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <InfoItem label="Email" value={user.email} />
                <InfoItem label="Số điện thoại" value={user.phoneNumber || "Chưa cập nhật"} />
                <InfoItem label="Địa chỉ" value={user.address || "Chưa cập nhật"} className="col-span-full" />
                <InfoItem label="Ngày tham gia" value={new Date(user.createdAt).toLocaleDateString("vi-VN")} />
                <InfoItem label="Cập nhật lần cuối" value={user.updatedAt ? new Date(user.updatedAt).toLocaleDateString("vi-VN") : "Chưa cập nhật"} />
              </div>
            </div>
          )}
          
          {activeTab !== "profile" && loading && (
            <div className="flex items-center justify-center py-12">
              <div className="w-8 h-8 border-4 border-orange-200 border-t-orange-500 rounded-full animate-spin"></div>
            </div>
          )}
          
          {activeTab === "properties" && !loading && (
            <div className="space-y-4">
              {properties.length === 0 ? <EmptyState label="Chưa có phòng trọ nào." /> : properties.map(p => (
                <div key={p.id} className="bg-white p-4 rounded-2xl border border-gray-100 shadow-sm flex justify-between items-center">
                  <div>
                    <Link to={`/rooms/${p.id}`} target="_blank" className="font-semibold text-blue-600 hover:underline text-sm block mb-1">
                      {p.propertyName}
                    </Link>
                    <p className="text-xs text-gray-500">{p.address}</p>
                  </div>
                  <span className="px-2 py-1 bg-gray-100 text-gray-600 text-xs rounded-lg font-medium">{p.status}</span>
                </div>
              ))}
            </div>
          )}
          
          {activeTab === "appointments" && !loading && (
            <div className="space-y-4">
              {appointments.length === 0 ? <EmptyState label="Chưa có lịch hẹn nào." /> : appointments.map(a => (
                <div key={a.id} className="bg-white p-4 rounded-2xl border border-gray-100 shadow-sm">
                  <div className="flex justify-between mb-2">
                    <span className="text-sm font-semibold text-gray-900">{new Date(a.appointmentDateTime).toLocaleString("vi-VN")}</span>
                    <span className="px-2 py-1 bg-gray-100 text-gray-600 text-xs rounded-lg font-medium">{a.status}</span>
                  </div>
                  <p className="text-xs text-gray-500 mb-2">
                    Phòng: <Link to={`/rooms/${a.propertyId}`} target="_blank" className="text-blue-500 hover:underline">{propertyNames[a.propertyId] || a.propertyId}</Link>
                  </p>
                  {a.note && <p className="text-xs text-gray-600 bg-gray-50 p-2 rounded-lg italic">"{a.note}"</p>}
                </div>
              ))}
            </div>
          )}
          
          {activeTab === "complaints" && !loading && (
            <div className="space-y-4">
              {complaints.length === 0 ? <EmptyState label="Chưa có khiếu nại nào." /> : complaints.map(c => (
                <div key={c.id} className="bg-white p-4 rounded-2xl border border-gray-100 shadow-sm">
                  <div className="flex justify-between items-start mb-2">
                    <h4 className="font-semibold text-gray-900 text-sm flex-1">{c.title}</h4>
                    <span className="px-2 py-1 bg-gray-100 text-gray-600 text-xs rounded-lg font-medium ml-2">{c.status}</span>
                  </div>
                  <p className="text-xs text-gray-600 line-clamp-2 mb-2">{c.content}</p>
                  <p className="text-xs text-gray-400">{new Date(c.createdAt).toLocaleDateString("vi-VN")}</p>
                </div>
              ))}
            </div>
          )}
          
          {activeTab === "ratings" && !loading && (
            <div className="space-y-4">
              {ratings.length === 0 ? <EmptyState label="Chưa có đánh giá nào." /> : ratings.map(r => (
                <div key={r.id} className="bg-white p-4 rounded-2xl border border-gray-100 shadow-sm">
                  <div className="flex justify-between items-center mb-2">
                    <div className="flex gap-1 text-yellow-400">
                      {Array.from({ length: 5 }).map((_, i) => (
                        <Star key={i} className={`w-4 h-4 ${i < r.stars ? "fill-current" : "text-gray-200"}`} />
                      ))}
                    </div>
                    <span className="text-xs text-gray-400">{new Date(r.createdAt).toLocaleDateString("vi-VN")}</span>
                  </div>
                  <p className="text-xs text-gray-500 mb-2">
                    Phòng: <Link to={`/rooms/${r.propertyId}`} target="_blank" className="text-blue-500 hover:underline">{propertyNames[r.propertyId] || r.propertyId}</Link>
                  </p>
                  <p className="text-sm text-gray-700 italic">"{r.content}"</p>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </>
  );
}

function TabButton({ active, onClick, icon, label }: { active: boolean; onClick: () => void; icon: React.ReactNode; label: string }) {
  return (
    <button
      onClick={onClick}
      className={`flex items-center gap-2 px-4 py-3 border-b-2 text-sm font-medium transition-colors whitespace-nowrap ${
        active ? "border-orange-500 text-orange-600" : "border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-200"
      }`}
    >
      {icon}
      {label}
    </button>
  );
}

function InfoItem({ label, value, className = "" }: { label: string; value: string; className?: string }) {
  return (
    <div className={`flex flex-col gap-1 ${className}`}>
      <span className="text-xs font-semibold text-gray-500 uppercase tracking-wider">{label}</span>
      <span className="text-sm text-gray-900 font-medium">{value}</span>
    </div>
  );
}

function EmptyState({ label }: { label: string }) {
  return (
    <div className="text-center py-12 text-gray-400 bg-white rounded-2xl border border-dashed border-gray-200">
      <FileText className="w-8 h-8 mx-auto mb-2 opacity-40" />
      <p className="text-sm">{label}</p>
    </div>
  );
}
