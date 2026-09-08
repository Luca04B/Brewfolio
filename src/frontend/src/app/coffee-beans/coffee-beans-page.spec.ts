import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { of, Subject, throwError } from 'rxjs';
import { CoffeeBean, CoffeeBeansApi } from './coffee-beans-api';
import { CoffeeBeansPage } from './coffee-beans-page';

const BEAN: CoffeeBean = {
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

describe('CoffeeBeansPage', () => {
  let fixture: ComponentFixture<CoffeeBeansPage>;
  let api: {
    list: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    api = {
      list: vi.fn().mockReturnValue(of({ items: [], nextCursor: null })),
    };

    await TestBed.configureTestingModule({
      imports: [CoffeeBeansPage],
      providers: [provideRouter([]), { provide: CoffeeBeansApi, useValue: api }],
    }).compileComponents();
  });

  it('shows the empty collection state after loading', () => {
    fixture = TestBed.createComponent(CoffeeBeansPage);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[data-testid="empty-state"]')).toBeTruthy();
  });

  it('shows progress while the collection request is pending', () => {
    api.list.mockReturnValue(new Subject<{ items: CoffeeBean[]; nextCursor: string | null }>());
    fixture = TestBed.createComponent(CoffeeBeansPage);
    fixture.detectChanges();

    expect(fixture.nativeElement.querySelector('[role="status"]')?.textContent).toBeTruthy();
  });

  it('offers retry after the collection request fails', () => {
    api.list
      .mockReturnValueOnce(throwError(() => new Error('offline')))
      .mockReturnValueOnce(of({ items: [BEAN], nextCursor: null }));
    fixture = TestBed.createComponent(CoffeeBeansPage);
    fixture.detectChanges();

    fixture.debugElement.query(By.css('[data-testid="retry"]')).triggerEventHandler('click');
    fixture.detectChanges();

    expect(api.list).toHaveBeenCalledTimes(2);
    expect(fixture.nativeElement.querySelector('[data-testid="bean-name"]')?.textContent).toContain(
      'Ethiopia Bombe',
    );
  });
});
