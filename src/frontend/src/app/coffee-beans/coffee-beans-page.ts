import { Component, HostListener, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CoffeeBean, CoffeeBeansApi, RoastLevel } from './coffee-beans-api';
import { LanguageService } from './language.service';

type CollectionState = 'loading' | 'ready' | 'error';

const COPY = {
  de: {
    navCollection: 'Coffee Beans',
    add: 'Coffee Bean anlegen',
    eyebrow: 'Deine Kaffeebibliothek',
    headline: 'Jede Bohne hat eine Geschichte.',
    intro:
      'Sammle deine Lieblingskaffees als Produkte. Bags, Röstdaten und Vorräte kommen im nächsten Schritt dazu.',
    products: 'Produkte',
    collectionEyebrow: 'Sammlung',
    collectionTitle: 'Deine Coffee Beans',
    loading: 'Coffee Beans werden geladen …',
    emptyTitle: 'Deine Sammlung wartet auf die erste Bohne.',
    emptyText: 'Lege unten Name und Rösterei an – mehr braucht es für den Anfang nicht.',
    errorTitle: 'Die Sammlung konnte nicht geladen werden.',
    errorText: 'Prüfe die Verbindung und versuche es noch einmal.',
    retry: 'Erneut versuchen',
    added: 'Hinzugefügt',
    formEyebrow: 'Neues Produkt',
    formTitle: 'Coffee Bean anlegen',
    formText:
      'Eine Coffee Bean beschreibt das wiederverwendbare Produkt, nicht eine einzelne Packung.',
    name: 'Name',
    namePlaceholder: 'z. B. Ethiopia Bombe',
    roaster: 'Rösterei',
    roasterPlaceholder: 'z. B. Example Roasters',
    required: 'Dieses Feld ist erforderlich.',
    tooLong: 'Maximal 120 Zeichen.',
    save: 'Coffee Bean speichern',
    saving: 'Wird gespeichert …',
    saveError: 'Die Coffee Bean konnte nicht gespeichert werden. Bitte versuche es erneut.',
    search: 'Name oder Rösterei suchen',
    filter: 'Vorrat',
    all: 'Alle',
    inStock: 'Im Vorrat',
    outOfStock: 'Nicht im Vorrat',
    roast: 'Röstgrad',
    sort: 'Sortierung',
    recent: 'Letzte Aktivität',
    nameSort: 'Name',
    activity: 'Letztes Röstdatum',
    loadMore: 'Mehr laden',
    loadingMore: 'Weitere Coffee Beans werden geladen …',
    end: 'Das war das ganze Regal.',
    endText: 'Du bist mit deiner Sammlung auf dem neuesten Stand.',
    retryMore: 'Laden erneut versuchen',
    deleted: 'Coffee Bean wurde gelöscht.',
    noResults: 'Keine Coffee Beans passen zu deinen Filtern.',
  },
  en: {
    navCollection: 'Coffee Beans',
    add: 'Add Coffee Bean',
    eyebrow: 'Your coffee library',
    headline: 'Every bean has a story.',
    intro:
      'Collect your favorite coffees as products. Bags, roast dates, and stock will follow in the next step.',
    products: 'Products',
    collectionEyebrow: 'Collection',
    collectionTitle: 'Your Coffee Beans',
    loading: 'Loading Coffee Beans …',
    emptyTitle: 'Your collection is ready for its first bean.',
    emptyText: 'Start with a name and roaster below – that is all you need for now.',
    errorTitle: 'The collection could not be loaded.',
    errorText: 'Check the connection and try again.',
    retry: 'Try again',
    added: 'Added',
    formEyebrow: 'New product',
    formTitle: 'Add Coffee Bean',
    formText: 'A Coffee Bean describes the reusable product, not an individual bag.',
    name: 'Name',
    namePlaceholder: 'e.g. Ethiopia Bombe',
    roaster: 'Roaster',
    roasterPlaceholder: 'e.g. Example Roasters',
    required: 'This field is required.',
    tooLong: 'Maximum 120 characters.',
    save: 'Save Coffee Bean',
    saving: 'Saving …',
    saveError: 'The Coffee Bean could not be saved. Please try again.',
    search: 'Search name or roaster',
    filter: 'Stock',
    all: 'All',
    inStock: 'In stock',
    outOfStock: 'Out of stock',
    roast: 'Roast level',
    sort: 'Sort',
    recent: 'Latest activity',
    nameSort: 'Name',
    activity: 'Latest roast date',
    loadMore: 'Load more',
    loadingMore: 'Loading more Coffee Beans …',
    end: 'That’s the whole shelf.',
    endText: 'You are all caught up with your collection.',
    retryMore: 'Retry loading',
    deleted: 'Coffee Bean deleted.',
    noResults: 'No Coffee Beans match your filters.',
  },
} as const;

@Component({
  imports: [RouterLink],
  selector: 'app-coffee-beans-page',
  styleUrl: './coffee-beans-page.scss',
  templateUrl: './coffee-beans-page.html',
})
export class CoffeeBeansPage implements OnInit {
  private readonly api = inject(CoffeeBeansApi);
  private readonly languages = inject(LanguageService);

  readonly language = this.languages.current;
  readonly copy = computed(() => COPY[this.language()]);
  readonly beans = signal<readonly CoffeeBean[]>([]);
  readonly collectionState = signal<CollectionState>('loading');
  readonly nextCursor = signal<string | null>(null);
  readonly loadingMore = signal(false);
  readonly nextPageFailed = signal(false);
  readonly reachedEnd = signal(false);
  readonly failedImages = signal<ReadonlySet<string>>(new Set());
  readonly success = signal(history.state?.message === 'deleted');
  readonly search = signal('');
  readonly stock = signal<'all' | 'true' | 'false'>('all');
  readonly roastLevel = signal<RoastLevel | 'all'>('all');
  readonly sort = signal<'LatestActivity' | 'Name' | 'LatestRoastDate'>('LatestActivity');
  readonly roastLevels: readonly RoastLevel[] = [
    'Unknown',
    'Light',
    'MediumLight',
    'Medium',
    'MediumDark',
    'Dark',
  ];
  private searchTimer?: ReturnType<typeof setTimeout>;

  ngOnInit(): void {
    this.load();
  }

  load(append = false): void {
    if (append && (this.loadingMore() || !this.nextCursor())) return;
    if (append) this.loadingMore.set(true);
    else this.collectionState.set('loading');
    this.nextPageFailed.set(false);
    const roastLevel = this.roastLevel();
    this.api
      .list({
        search: this.search().trim() || undefined,
        isInStock: this.stock() === 'all' ? undefined : this.stock() === 'true',
        roastLevel: roastLevel === 'all' ? undefined : roastLevel,
        sort: this.sort(),
        cursor: append ? (this.nextCursor() ?? undefined) : undefined,
      })
      .subscribe({
        next: (page) => {
          this.beans.set(append ? [...this.beans(), ...page.items] : page.items);
          this.nextCursor.set(page.nextCursor);
          this.reachedEnd.set(page.nextCursor === null);
          this.loadingMore.set(false);
          this.collectionState.set('ready');
        },
        error: () => {
          this.loadingMore.set(false);
          if (append) this.nextPageFailed.set(true);
          else this.collectionState.set('error');
        },
      });
  }

  searchChanged(value: string): void {
    this.search.set(value);
    clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => this.load(), 250);
  }

  filtersChanged(): void {
    this.reachedEnd.set(false);
    this.load();
  }

  @HostListener('window:scroll')
  loadNearEnd(): void {
    if (
      globalThis.innerHeight + globalThis.scrollY >=
      globalThis.document.body.offsetHeight - 500
    ) {
      this.load(true);
    }
  }

  toggleLanguage(): void {
    this.languages.toggle();
  }

  initials(coffeeBean: CoffeeBean): string {
    return `${coffeeBean.roaster.charAt(0)}${coffeeBean.name.charAt(0)}`.toUpperCase();
  }

  imageFailed(id: string): void {
    this.failedImages.update((current) => new Set([...current, id]));
  }

  formatDate(value: string | null): string {
    if (!value) return '—';
    return new Intl.DateTimeFormat(this.language(), {
      dateStyle: 'medium',
      timeZone: 'UTC',
    }).format(new Date(`${value.slice(0, 10)}T00:00:00Z`));
  }

  formatPrice(value: number | null): string {
    return value === null
      ? '—'
      : new Intl.NumberFormat(this.language(), { style: 'currency', currency: 'EUR' }).format(
          value,
        );
  }
}
