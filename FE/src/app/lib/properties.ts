import { apiRequest } from "./api";
import type {
  AmenityResponse,
  PagedResponse,
  PropertyImageResponse,
  PropertyListing,
  PropertyResponse,
  RoomAmenityResponse,
} from "./types";

const UTILITY_META_PREFIX = "[[UTILITY_META]]";

function parseUtilityMeta(description?: string | null) {
  if (!description) {
    return {
      cleanDescription: "",
      electricPrice: null,
      waterPrice: null,
    };
  }

  const markerIndex = description.indexOf(UTILITY_META_PREFIX);
  if (markerIndex === -1) {
    return {
      cleanDescription: description,
      electricPrice: null,
      waterPrice: null,
    };
  }

  const cleanDescription = description.slice(0, markerIndex).trim();
  const rawJson = description.slice(markerIndex + UTILITY_META_PREFIX.length).trim();

  try {
    const parsed = JSON.parse(rawJson) as { electricPrice?: number | null; waterPrice?: number | null };
    return {
      cleanDescription,
      electricPrice: parsed.electricPrice ?? null,
      waterPrice: parsed.waterPrice ?? null,
    };
  } catch {
    return {
      cleanDescription: description,
      electricPrice: null,
      waterPrice: null,
    };
  }
}

function attachUtilityMeta<T extends PropertyResponse>(property: T): T {
  const meta = parseUtilityMeta(property.description);
  return {
    ...property,
    description: meta.cleanDescription,
    electricPrice: property.electricPrice ?? null,
    waterPrice: property.waterPrice ?? null,
  };
}

function encodePropertyDescription(input: {
  description?: string;
  electricPrice?: number | null;
  waterPrice?: number | null;
}) {
  const cleanDescription = input.description?.trim() || "";
  const hasUtilityValues = input.electricPrice !== undefined || input.waterPrice !== undefined;

  if (!hasUtilityValues) {
    return cleanDescription;
  }

  const payload = JSON.stringify({
    electricPrice: input.electricPrice ?? null,
    waterPrice: input.waterPrice ?? null,
  });

  return cleanDescription ? `${cleanDescription}\n\n${UTILITY_META_PREFIX}${payload}` : `${UTILITY_META_PREFIX}${payload}`;
}

export async function getProperties(
  query: Record<string, string | number | boolean | undefined> = {},
  token?: string
) {
  // Map frontend 'status' to backend 'availabilityStatus' for EntityFilter
  const { status, ...restQuery } = query;
  const apiQuery = { ...restQuery };
  if (status) {
    apiQuery.availabilityStatus = status;
  }

  return apiRequest<PagedResponse<PropertyResponse>>("Property/GetPropertiesByFilter", {
    ...(token ? { authToken: token } : {}),
    query: apiQuery,
  });
}

export async function getModerationProperties(
  token: string,
  query: Record<string, string | number | boolean | undefined> = {},
) {
  return apiRequest<PagedResponse<PropertyResponse>>("Property/GetModerationProperties", {
    authToken: token,
    query,
  });
}

export async function getPropertyById(id: string) {
  const property = await apiRequest<PropertyResponse>("Property/GetPropertyById", {
    query: { id },
  });
  return attachUtilityMeta(property);
}

export async function getPropertyImages(propertyId: string) {
  // Use the dedicated endpoint that explicitly filters by property (more reliable than the general filter endpoint)
  const images = await apiRequest<PropertyImageResponse[]>("PropertyImage/GetPropertyImagesByPropertyId", {
    query: { propertyId },
  });
  return images || [];
}

export async function createPropertyImage(
  token: string,
  input: {
    propertyId: string;
    file: File;
    isPrimary?: boolean;
  },
) {
  const form = new FormData();
  form.append("propertyId", input.propertyId);
  form.append("file", input.file);
  form.append("isPrimary", String(Boolean(input.isPrimary)));

  return apiRequest<PropertyImageResponse>("PropertyImage/CreatePropertyImage", {
    method: "POST",
    authToken: token,
    body: form,
  });
}

export async function updatePropertyImage(
  token: string,
  input: {
    id: string;
    isPrimary?: boolean;
  },
) {
  return apiRequest<void>("PropertyImage/UpdatePropertyImage", {
    method: "PUT",
    authToken: token,
    body: input,
  });
}

export async function replacePropertyImage(
  token: string,
  input: {
    id: string;
    file: File;
  },
) {
  const form = new FormData();
  form.append("id", input.id);
  form.append("file", input.file);

  return apiRequest<PropertyImageResponse>("PropertyImage/ReplacePropertyImage", {
    method: "PUT",
    authToken: token,
    body: form,
  });
}

export async function deletePropertyImage(token: string, id: string) {
  return apiRequest<void>("PropertyImage/DeletePropertyImage", {
    method: "DELETE",
    authToken: token,
    query: { id },
  });
}

export async function getPropertyAmenities(propertyId: string) {
  const response = await apiRequest<PagedResponse<RoomAmenityResponse>>("RoomAmenity/GetRoomAmenitiesByFilter", {
    query: { propertyId, pageSize: 100 },
  });
  return response.items;
}

export async function createPropertyAmenity(
  token: string,
  input: {
    propertyId: string;
    amenityId: string;
    status?: string;
    note?: string;
  },
) {
  return apiRequest<RoomAmenityResponse>("RoomAmenity/CreateRoomAmenity", {
    method: "POST",
    authToken: token,
    body: input,
  });
}

export async function deletePropertyAmenity(token: string, id: string) {
  return apiRequest<void>("RoomAmenity/DeleteRoomAmenity", {
    method: "DELETE",
    authToken: token,
    body: { id },
  });
}

// Uses the richer backend endpoint (includes images + amenities in one roundtrip)
async function getPropertyDetail(id: string) {
  return apiRequest<any>("Property/GetPropertyDetailById", {
    query: { id },
  });
}

export async function getPropertyListing(id: string): Promise<PropertyListing> {
  const detail = await getPropertyDetail(id);

  // Support both camelCase (from JSON) and Pascal (defensive)
  const rawAmenities: any[] = detail.roomAmenities || detail.RoomAmenities || [];
  const amenities = rawAmenities.map((item) => item.amenityName ?? item.AmenityName);

  // Use embedded images from PropertyDetail when available (efficient).
  // Fall back to dedicated PropertyImage/GetPropertyImagesByFilter if the embedded list is empty.
  // This ensures images always appear in chi tiết and listing views.
  let images: string[] = [];
  const rawImages: any[] = detail.propertyImages || detail.PropertyImages || [];
  if (rawImages.length > 0) {
    images = rawImages
      .sort((a, b) => Number(b.isPrimary ?? b.IsPrimary) - Number(a.isPrimary ?? a.IsPrimary))
      .map((item) => item.imageUrl ?? item.ImageUrl);
  } else {
    try {
      const imgResponses = await getPropertyImages(id);
      images = imgResponses
        .sort((a, b) => Number(b.isPrimary) - Number(a.isPrimary))
        .map((item) => item.imageUrl);
    } catch {
      // leave images as []
    }
  }

  return {
    ...attachUtilityMeta(detail as PropertyResponse),
    images,
    amenities,
    landlord: detail.landlord || detail.Landlord,
  };
}

export async function getPropertyListings(
  query: Record<string, string | number | boolean | undefined> = {},
  token?: string
) {
  const response = await getProperties(query, token);
  const listings = await Promise.all(response.items.map((item) => getPropertyListing(item.id)));

  return {
    ...response,
    items: listings,
  };
}

export async function getRecommendedProperties(
  token: string,
  query: Record<string, string | number | boolean | undefined> = {},
) {
  return apiRequest<PagedResponse<PropertyResponse>>("Property/GetRecommendedProperties", {
    authToken: token,
    query: { pageSize: 12, ...query },
  });
}

export async function getRecommendedPropertyListings(
  token: string,
  query: Record<string, string | number | boolean | undefined> = {},
) {
  const response = await getRecommendedProperties(token, query);
  const listings = await Promise.all(response.items.map((item) => getPropertyListing(item.id)));

  return {
    ...response,
    items: listings,
  };
}

export async function getMostViewedProperties(
  token?: string,
  query: Record<string, string | number | boolean | undefined> = {},
) {
  return apiRequest<PagedResponse<PropertyResponse>>("Property/GetMostViewedProperties", {
    ...(token ? { authToken: token } : {}),
    query: { pageSize: 12, ...query },
  });
}

export async function getMostViewedPropertyListings(
  token?: string,
  query: Record<string, string | number | boolean | undefined> = {},
) {
  const response = await getMostViewedProperties(token, query);
  const listings = await Promise.all(response.items.map((item) => getPropertyListing(item.id)));

  return {
    ...response,
    items: listings,
  };
}

export async function getTrendingProperties(
  token?: string,
  query: Record<string, string | number | boolean | undefined> = {},
) {
  return apiRequest<PagedResponse<PropertyResponse>>("Property/GetTrendingProperties", {
    ...(token ? { authToken: token } : {}),
    query: { pageSize: 12, ...query },
  });
}

export async function getTrendingPropertyListings(
  token?: string,
  query: Record<string, string | number | boolean | undefined> = {},
) {
  const response = await getTrendingProperties(token, query);
  const listings = await Promise.all(response.items.map((item) => getPropertyListing(item.id)));

  return {
    ...response,
    items: listings,
  };
}

export async function getPopularPriceRanges(token?: string) {
  return apiRequest<any>("Property/GetPopularPriceRanges", {
    ...(token ? { authToken: token } : {}),
  });
}

export async function createProperty(
  token: string,
  input: {
    landlordId: string;
    areaId?: string | null;
    propertyName: string;
    address?: string;
    latitude?: number | null;
    longitude?: number | null;
    size: number;
    description?: string;
    price: number;
    status?: string;
    moderationStatus?: string;
    electricPrice?: number | null;
    waterPrice?: number | null;
  },
) {
  const electricPrice = Number(input.electricPrice ?? 0);
  const waterPrice = Number(input.waterPrice ?? 0);

  return apiRequest<PropertyResponse>("Property/CreateProperty", {
    method: "POST",
    authToken: token,
    body: {
      ...input,
      electricPrice,
      waterPrice,
      description: encodePropertyDescription({ ...input, electricPrice, waterPrice }),
    },
  });
}

export async function updateProperty(
  token: string,
  input: {
    id: string;
    areaId?: string | null;
    propertyName?: string;
    address?: string;
    latitude?: number | null;
    longitude?: number | null;
    size?: number;
    description?: string;
    price?: number;
    status?: string;
    moderationStatus?: string;
    rejectionReason?: string;
    electricPrice?: number | null;
    waterPrice?: number | null;
  },
) {
  const electricPrice = input.electricPrice != null ? Number(input.electricPrice) : undefined;
  const waterPrice = input.waterPrice != null ? Number(input.waterPrice) : undefined;

  return apiRequest<void>("Property/UpdateProperty", {
    method: "PUT",
    authToken: token,
    body: {
      ...input,
      ...(electricPrice !== undefined ? { electricPrice } : {}),
      ...(waterPrice !== undefined ? { waterPrice } : {}),
      description: encodePropertyDescription({
        ...input,
        electricPrice: electricPrice ?? null,
        waterPrice: waterPrice ?? null,
      }),
    },
  });
}

export async function deleteProperty(token: string, id: string) {
  return apiRequest<void>("Property/DeleteProperty", {
    method: "DELETE",
    authToken: token,
    body: { id },
  });
}

export async function approveProperty(token: string, id: string) {
  return apiRequest<void>("Property/ApproveProperty", {
    method: "POST",
    authToken: token,
    body: { propertyId: id },
  });
}

export async function rejectProperty(token: string, id: string, rejectionReason: string) {
  return apiRequest<void>("Property/RejectProperty", {
    method: "POST",
    authToken: token,
    body: { propertyId: id, rejectionReason },
  });
}
