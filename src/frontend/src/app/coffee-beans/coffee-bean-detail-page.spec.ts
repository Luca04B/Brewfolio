import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { NEVER, of } from 'rxjs';
import { CoffeeBean, CoffeeBeansApi } from './coffee-beans-api';
import { CoffeeBeanDetailPage } from './coffee-bean-detail-page';

const BEAN: CoffeeBean = {
  id: 'f723c39e-7f7a-48b0-a234-58387d3efc49',
  name: 'Ethiopia Bombe',
  roaster: 'Example Roasters',
  origin: 'Ethiopia',
  roastLevel: 'MediumLight',
  description: 'Floral and juicy.',
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

describe('CoffeeBeanDetailPage', () => {
  it('shows the complete product profile and a safe external link', async () => {
    await TestBed.configureTestingModule({
      imports: [CoffeeBeanDetailPage],
      providers: [
        provideRouter([]),
        { provide: CoffeeBeansApi, useValue: { get: () => of(BEAN) } },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ id: BEAN.id }) } },
        },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(CoffeeBeanDetailPage);
    fixture.detectChanges();
    const root = fixture.nativeElement as HTMLElement;

    expect(root.textContent).toContain('Ethiopia Bombe');
    expect(root.textContent).toContain('Ethiopia');
    expect(root.textContent).toContain('MediumLight');
    const link = root.querySelector('a[data-testid="product-link"]');
    expect(link?.getAttribute('target')).toBe('_blank');
    expect(link?.getAttribute('rel')).toContain('noopener');
  });

  it('uses a custom confirmation dialog for destructive actions', async () => {
    const deleteRequest = vi.fn().mockReturnValue(NEVER);
    await TestBed.configureTestingModule({
      imports: [CoffeeBeanDetailPage],
      providers: [
        provideRouter([]),
        { provide: CoffeeBeansApi, useValue: { get: () => of(BEAN), delete: deleteRequest } },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ id: BEAN.id }) } },
        },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(CoffeeBeanDetailPage);
    fixture.detectChanges();
    const root = fixture.nativeElement as HTMLElement;
    const actionLabels = [
      ...root.querySelectorAll<HTMLButtonElement>('.product-actions button'),
    ].map((button) => button.textContent?.trim());

    expect(actionLabels).not.toContain('Duplicate');
    expect(actionLabels).not.toContain('Duplizieren');
    root.querySelector<HTMLButtonElement>('[data-testid="delete-coffee-bean"]')?.click();
    fixture.detectChanges();

    expect(root.querySelector('[role="alertdialog"]')).toBeTruthy();
    expect(deleteRequest).not.toHaveBeenCalled();

    root.querySelector<HTMLButtonElement>('[data-testid="confirm-action"]')?.click();
    await fixture.whenStable();
    fixture.detectChanges();

    expect(deleteRequest).toHaveBeenCalledWith(BEAN.id);
    expect(root.querySelector('[role="alertdialog"]')).toBeFalsy();
  });
});
