import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';
import { CoffeeBean, CoffeeBeansApi } from './coffee-beans-api';
import { CoffeeBeanCreatePage } from './coffee-bean-create-page';

const CREATED: CoffeeBean = {
  id: 'f723c39e-7f7a-48b0-a234-58387d3efc49',
  name: 'Ethiopia Bombe',
  roaster: 'Example Roasters',
  origin: 'Ethiopia',
  roastLevel: 'Light',
  description: 'Floral',
  productUrl: 'https://example.com/coffee',
  imageUrl: null,
  imageThumbnailUrl: null,
  createdAt: '2026-09-04T10:00:00Z',
  updatedAt: '2026-09-04T10:00:00Z',
  bags: [],
  bagCount: 0,
  isInStock: false,
  latestRoastedOn: null,
  latestRoastAgeDays: null,
  latestOpenedOn: null,
  latestOpenAgeDays: null,
  weightedPricePer100Grams: null,
  latestActivityAt: '2026-09-04T10:00:00Z',
};

describe('CoffeeBeanCreatePage', () => {
  it('creates the complete product and navigates to its detail', async () => {
    const api = { create: vi.fn().mockReturnValue(of(CREATED)) };
    await TestBed.configureTestingModule({
      imports: [CoffeeBeanCreatePage],
      providers: [provideRouter([]), { provide: CoffeeBeansApi, useValue: api }],
    }).compileComponents();
    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
    const fixture = TestBed.createComponent(CoffeeBeanCreatePage);
    fixture.detectChanges();

    setInput(fixture.nativeElement, 'name', 'Ethiopia Bombe');
    setInput(fixture.nativeElement, 'roaster', 'Example Roasters');
    setInput(fixture.nativeElement, 'origin', 'Ethiopia');
    setSelect(fixture.nativeElement, 'roastLevel', 'Light');
    setInput(fixture.nativeElement, 'description', 'Floral');
    setInput(fixture.nativeElement, 'productUrl', 'https://example.com/coffee');
    fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit'));
    fixture.detectChanges();

    expect(api.create).toHaveBeenCalledWith(
      {
        name: 'Ethiopia Bombe',
        roaster: 'Example Roasters',
        origin: 'Ethiopia',
        roastLevel: 'Light',
        description: 'Floral',
        productUrl: 'https://example.com/coffee',
        firstBag: null,
        allowDuplicate: false,
      },
      undefined,
    );
    expect(router.navigate).toHaveBeenCalledWith(['/coffee-beans', CREATED.id], {
      state: { message: 'created' },
    });
  });
});

function setInput(root: HTMLElement, name: string, value: string): void {
  const input = root.querySelector(`[name="${name}"]`) as HTMLInputElement;
  input.value = value;
  input.dispatchEvent(new Event('input'));
}

function setSelect(root: HTMLElement, name: string, value: string): void {
  const select = root.querySelector(`[name="${name}"]`) as HTMLSelectElement;
  select.value = value;
  select.dispatchEvent(new Event('change'));
}
