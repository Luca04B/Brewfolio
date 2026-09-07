import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface CoffeeBean {
  id: string;
  name: string;
  roaster: string;
  origin: string | null;
  roastLevel: RoastLevel;
  description: string | null;
  productUrl: string | null;
  imageUrl: string | null;
  imageThumbnailUrl: string | null;
  createdAt: string;
  updatedAt: string;
  bags: CoffeeBag[];
  bagCount: number;
  isInStock: boolean;
  latestRoastedOn: string | null;
  latestRoastAgeDays: number | null;
  latestOpenedOn: string | null;
  latestOpenAgeDays: number | null;
  weightedPricePer100Grams: number | null;
  latestActivityAt: string;
}

export interface CoffeeBag {
  id: string;
  purchasedOn: string;
  roastedOn: string | null;
  openedOn: string | null;
  initialWeightGrams: number;
  pricePaid: number;
  isInStock: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CoffeeBagInput {
  purchasedOn: string;
  roastedOn: string | null;
  openedOn: string | null;
  initialWeightGrams: number;
  pricePaid: number;
  isInStock: boolean;
}

export type RoastLevel = 'Unknown' | 'Light' | 'MediumLight' | 'Medium' | 'MediumDark' | 'Dark';

export interface CreateCoffeeBeanRequest {
  name: string;
  roaster: string;
  origin: string | null;
  roastLevel: RoastLevel;
  description: string | null;
  productUrl: string | null;
  firstBag: CoffeeBagInput | null;
  allowDuplicate?: boolean;
}

export interface CoffeeBeanPage {
  items: CoffeeBean[];
  nextCursor: string | null;
}

export interface CoffeeBeanQuery {
  search?: string;
  isInStock?: boolean;
  roastLevel?: RoastLevel;
  sort?: 'LatestActivity' | 'Name' | 'LatestRoastDate';
  cursor?: string;
  limit?: number;
}

@Injectable({ providedIn: 'root' })
export class CoffeeBeansApi {
  private readonly http = inject(HttpClient);
  private readonly collectionUrl = '/api/coffee-beans';

  list(query: CoffeeBeanQuery = {}): Observable<CoffeeBeanPage> {
    const params = Object.fromEntries(
      Object.entries(query)
        .filter(([, value]) => value !== undefined && value !== '')
        .map(([key, value]) => [key, String(value)]),
    );
    return this.http.get<CoffeeBeanPage>(this.collectionUrl, { params });
  }

  create(request: CreateCoffeeBeanRequest, image?: File): Observable<CoffeeBean> {
    if (!image) {
      return this.http.post<CoffeeBean>(this.collectionUrl, request);
    }

    const form = new FormData();
    form.set('data', JSON.stringify(request));
    form.set('image', image);
    return this.http.post<CoffeeBean>(this.collectionUrl, form);
  }

  get(id: string): Observable<CoffeeBean> {
    return this.http.get<CoffeeBean>(`${this.collectionUrl}/${id}`);
  }

  createBag(coffeeBeanId: string, input: CoffeeBagInput): Observable<CoffeeBean> {
    return this.http.post<CoffeeBean>(`${this.collectionUrl}/${coffeeBeanId}/bags`, input);
  }

  updateBag(
    coffeeBeanId: string,
    coffeeBagId: string,
    input: CoffeeBagInput,
  ): Observable<CoffeeBean> {
    return this.http.put<CoffeeBean>(
      `${this.collectionUrl}/${coffeeBeanId}/bags/${coffeeBagId}`,
      input,
    );
  }

  setBagStock(
    coffeeBeanId: string,
    coffeeBagId: string,
    isInStock: boolean,
  ): Observable<CoffeeBean> {
    return this.http.put<CoffeeBean>(
      `${this.collectionUrl}/${coffeeBeanId}/bags/${coffeeBagId}/stock`,
      { isInStock },
    );
  }

  openBag(coffeeBeanId: string, coffeeBagId: string): Observable<CoffeeBean> {
    return this.http.post<CoffeeBean>(
      `${this.collectionUrl}/${coffeeBeanId}/bags/${coffeeBagId}/open`,
      null,
    );
  }

  deleteBag(coffeeBeanId: string, coffeeBagId: string): Observable<CoffeeBean> {
    return this.http.delete<CoffeeBean>(
      `${this.collectionUrl}/${coffeeBeanId}/bags/${coffeeBagId}`,
    );
  }

  update(id: string, request: Omit<CreateCoffeeBeanRequest, 'firstBag'>): Observable<CoffeeBean> {
    return this.http.put<CoffeeBean>(`${this.collectionUrl}/${id}`, request);
  }

  duplicate(id: string): Observable<CoffeeBean> {
    return this.http.post<CoffeeBean>(`${this.collectionUrl}/${id}/duplicate`, {
      acknowledged: true,
    });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.collectionUrl}/${id}`);
  }

  replaceImage(id: string, image: File): Observable<CoffeeBean> {
    const form = new FormData();
    form.set('image', image);
    return this.http.put<CoffeeBean>(`${this.collectionUrl}/${id}/image`, form);
  }

  removeImage(id: string): Observable<CoffeeBean> {
    return this.http.delete<CoffeeBean>(`${this.collectionUrl}/${id}/image`);
  }
}
