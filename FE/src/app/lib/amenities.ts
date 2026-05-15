import { apiRequest } from "./api";
import type { AmenityResponse } from "./types";

export async function getAmenities() {
  return apiRequest<AmenityResponse[]>("Amenity/all");
}
