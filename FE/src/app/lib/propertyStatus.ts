const availableStatuses = new Set(["available"]);
const rentedStatuses = new Set(["rented"]);
const maintenanceStatuses = new Set(["maintenance"]);

export function isAvailablePropertyStatus(status: string) {
  return availableStatuses.has(status.toLowerCase());
}

export function isRentedPropertyStatus(status: string) {
  return rentedStatuses.has(status.toLowerCase());
}

export function isMaintenancePropertyStatus(status: string) {
  return maintenanceStatuses.has(status.toLowerCase());
}

export function getPropertyStatusMeta(status: string) {
  if (isAvailablePropertyStatus(status)) {
    return {
      label: "Trống",
      tone: "green" as const,
    };
  }

  if (isRentedPropertyStatus(status)) {
    return {
      label: "Đã cho thuê",
      tone: "blue" as const,
    };
  }

  if (isMaintenancePropertyStatus(status)) {
    return {
      label: "Đang sửa chữa",
      tone: "amber" as const,
    };
  }

  return {
    label: status,
    tone: "slate" as const,
  };
}
