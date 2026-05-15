export function isOccupyingContractStatus(status: string) {
  const normalized = status.trim().toLowerCase();
  return normalized === "active" || normalized === "nearexpiry" || normalized === "signed" || normalized === "approved";
}
