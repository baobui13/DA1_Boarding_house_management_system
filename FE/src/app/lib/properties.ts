import { apiRequest } from "./api";
import type {
  PagedResponse,
  PropertyImageResponse,
  PropertyListing,
  PropertyResponse,
  RoomAmenityResponse,
} from "./types";

export async function getProperties(query: Record<string, string | number | boolean | undefined> = {}) {
  return apiRequest<PagedResponse<PropertyResponse>>("Property/GetPropertiesByFilter", {
    query,
  });
}

export async function getPropertyById(id: string) {
  return apiRequest<PropertyResponse>("Property/GetPropertyById", {
    query: { id },
  });
}

export async function getPropertyImages(propertyId: string) {
  const response = await apiRequest<PagedResponse<PropertyImageResponse>>("PropertyImage/GetPropertyImagesByFilter", {
    query: { propertyId, pageSize: 100 },
  });
  return response.items;
}

export async function getPropertyAmenities(propertyId: string) {
  const response = await apiRequest<PagedResponse<RoomAmenityResponse>>("RoomAmenity/GetRoomAmenitiesByFilter", {
    query: { roomId: propertyId, pageSize: 100 },
  });
  return response.items;
}

export async function getPropertyListing(id: string): Promise<PropertyListing> {
  const [property, images, amenities] = await Promise.all([
    getPropertyById(id),
    getPropertyImages(id),
    getPropertyAmenities(id),
  ]);

  return {
    ...property,
    images: images
      .sort((a, b) => Number(b.isPrimary) - Number(a.isPrimary))
      .map((item) => item.imageUrl),
    amenities: amenities.map((item) => item.amenityName),
  };
}

export async function getPropertyListings(query: Record<string, string | number | boolean | undefined> = {}) {
  const response = await getProperties({ pageSize: 100, ...query });
  const listings = await Promise.all(response.items.map((item) => getPropertyListing(item.id)));

  return {
    ...response,
    items: listings,
  };
}

export async function createProperty(
  token: string,
  input: {
    landlordId: string;
    propertyName: string;
    address?: string;
    size: number;
    description?: string;
    price: number;
    status?: string;
  },
) {
  return apiRequest<PropertyResponse>("Property/CreateProperty", {
    method: "POST",
    authToken: token,
    body: input,
  });
}

export async function updateProperty(
  token: string,
  input: {
    id: string;
    propertyName?: string;
    address?: string;
    size?: number;
    description?: string;
    price?: number;
    status?: string;
    rejectionReason?: string;
  },
) {
  return apiRequest<void>("Property/UpdateProperty", {
    method: "PUT",
    authToken: token,
    body: input,
  });
}

export async function deleteProperty(token: string, id: string) {
  return apiRequest<void>("Property/DeleteProperty", {
    method: "DELETE",
    authToken: token,
    body: { id },
  });
}
