import { FileText, AlertCircle, TriangleAlert } from "lucide-react";

export default function ContractTemplatesPage() {
  return (
    <div className="max-w-4xl mx-auto px-4 py-6">
      <div className="mb-6">
        <h1 className="text-gray-900" style={{ fontSize: "22px", fontWeight: 700 }}>
          Mẫu Hợp Đồng
        </h1>
        <p className="text-gray-500 mt-0.5" style={{ fontSize: "14px" }}>
          Trang này đã bỏ mock templates.
        </p>
      </div>

      <div className="bg-blue-50 border border-blue-100 rounded-xl p-4 mb-6">
        <div className="flex items-start gap-3">
          <AlertCircle className="w-5 h-5 text-blue-600 mt-0.5 shrink-0" />
          <div>
            <h4 className="text-blue-900 mb-1" style={{ fontSize: "14px", fontWeight: 600 }}>
              Trạng thái hiện tại
            </h4>
            <p className="text-blue-700" style={{ fontSize: "13px" }}>
              Backend hiện chưa có endpoint CRUD cho contract templates, nên frontend không thể lưu hay tải mẫu hợp đồng thật.
            </p>
          </div>
        </div>
      </div>

      <div className="bg-white rounded-2xl border border-gray-100 p-8 text-center">
        <FileText className="w-12 h-12 text-gray-300 mx-auto mb-4" />
        <h2 className="text-gray-900 mb-2" style={{ fontSize: "18px", fontWeight: 700 }}>
          Chưa có API cho mẫu hợp đồng
        </h2>
        <p className="text-gray-500 max-w-2xl mx-auto" style={{ fontSize: "14px", lineHeight: 1.7 }}>
          Để trang này hoạt động thật, backend cần một resource riêng kiểu `ContractTemplate` với các endpoint tối thiểu:
          `GetTemplates`, `GetTemplateById`, `CreateTemplate`, `UpdateTemplate`, `DeleteTemplate`, và có thể thêm `SetDefaultTemplate`.
        </p>

        <div className="mt-6 rounded-xl border border-amber-200 bg-amber-50 p-4 text-left">
          <div className="flex items-start gap-3">
            <TriangleAlert className="w-5 h-5 text-amber-600 mt-0.5 shrink-0" />
            <p className="text-amber-800" style={{ fontSize: "13px" }}>
              Hiện tại mọi thao tác template đã bị gỡ khỏi frontend để tránh người dùng tưởng dữ liệu đang được lưu thật.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
