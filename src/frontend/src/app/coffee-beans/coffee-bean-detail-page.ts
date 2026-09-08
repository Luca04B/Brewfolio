import { Component, HostListener, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import {
  CoffeeBag,
  CoffeeBagInput,
  CoffeeBean,
  CoffeeBeansApi,
  RoastLevel,
} from './coffee-beans-api';
import { LanguageService } from './language.service';

interface ConfirmationDialog {
  readonly confirmLabel: string;
  readonly message: string;
  readonly resolve: (confirmed: boolean) => void;
  readonly title: string;
  readonly tone: 'danger' | 'warning';
}

const COPY = {
  de: {
    loading: 'Coffee Bean wird geladen …',
    notFound: 'Coffee Bean nicht gefunden.',
    loadError: 'Die Coffee Bean konnte nicht geladen werden.',
    back: 'Zurück zur Sammlung',
    noDescription: 'Noch keine Beschreibung hinterlegt.',
    product: 'Produktseite öffnen ↗',
    profile: 'Produktprofil',
    provenance: 'Herkunft & Röstung',
    origin: 'Herkunft',
    roast: 'Röstgrad',
    created: 'Erstellt',
    updated: 'Aktualisiert',
    missing: 'Nicht angegeben',
    edit: 'Bearbeiten',
    delete: 'Löschen',
    bags: 'Packungen & Käufe',
    addBag: 'Coffee Bag',
    noBags: 'Noch keine Packung erfasst.',
    noBagsText: 'Deine Käufe, Röstdaten und Vorräte erscheinen hier.',
    inStock: 'Im Vorrat',
    outOfStock: 'Nicht im Vorrat',
    purchased: 'Gekauft',
    roasted: 'Geröstet',
    opened: 'Geöffnet',
    bag: 'Packung',
    openToday: 'Heute öffnen',
    toStock: 'In Vorrat',
    fromStock: 'Aus Vorrat',
    actionError: 'Die Aktion ist fehlgeschlagen. Deine vorherigen Daten bleiben erhalten.',
    cancel: 'Abbrechen',
    save: 'Speichern',
    imageSave: 'Bild speichern',
    successCreated: 'Coffee Bean wurde angelegt.',
    successUpdated: 'Änderungen wurden gespeichert.',
    imageReplace: 'Bild ersetzen',
    imageAdd: 'Bild hinzufügen',
    imageRemove: 'Bild entfernen',
    imagePresent: '1 Bild',
    noImage: 'Kein Bild',
    sensory:
      'Altershinweise beschreiben Geschmack und Aroma – nicht Sicherheit oder Genießbarkeit.',
    weight: 'Gramm',
    price: 'Preis €',
    purchaseDate: 'Kaufdatum',
    roastDate: 'Röstdatum',
    openedDate: 'Geöffnet am',
    stock: 'Im Vorrat',
    description: 'Beschreibung',
    productUrl: 'Produktlink',
    roaster: 'Rösterei',
    name: 'Name',
    actionSaveError: 'Änderungen konnten nicht gespeichert werden.',
    deleteBeanTitle: 'Coffee Bean löschen?',
    deleteBeanConfirm: 'Endgültig löschen',
    removeImageTitle: 'Produktbild entfernen?',
    removeImageText: 'Das aktuelle Bild wird aus dieser Coffee Bean entfernt.',
    removeImageConfirm: 'Bild entfernen',
    deleteBagTitle: 'Coffee Bag löschen?',
    deleteBagText: 'Die erfassten Kauf- und Röstdaten dieser Packung gehen verloren.',
    deleteBagConfirm: 'Coffee Bag löschen',
    discardTitle: 'Änderungen verwerfen?',
    discardText: 'Deine noch nicht gespeicherten Eingaben gehen verloren.',
    discardConfirm: 'Änderungen verwerfen',
    duplicateWarningTitle: 'Mögliche Dublette',
    duplicateWarningFallback: 'Eine ähnliche Coffee Bean ist bereits vorhanden.',
    saveAnyway: 'Trotzdem speichern',
  },
  en: {
    loading: 'Loading Coffee Bean …',
    notFound: 'Coffee Bean not found.',
    loadError: 'The Coffee Bean could not be loaded.',
    back: 'Back to collection',
    noDescription: 'No description yet.',
    product: 'Open product page ↗',
    profile: 'Product profile',
    provenance: 'Origin & roast',
    origin: 'Origin',
    roast: 'Roast level',
    created: 'Created',
    updated: 'Updated',
    missing: 'Not provided',
    edit: 'Edit',
    delete: 'Delete',
    bags: 'Bags & purchases',
    addBag: 'Coffee Bag',
    noBags: 'No bag recorded yet.',
    noBagsText: 'Purchases, roast dates, and stock will appear here.',
    inStock: 'In stock',
    outOfStock: 'Out of stock',
    purchased: 'Purchased',
    roasted: 'Roasted',
    opened: 'Opened',
    bag: 'Bag',
    openToday: 'Open today',
    toStock: 'Move to stock',
    fromStock: 'Remove from stock',
    actionError: 'The action failed. Your previous data remains intact.',
    cancel: 'Cancel',
    save: 'Save',
    imageSave: 'Save image',
    successCreated: 'Coffee Bean created.',
    successUpdated: 'Changes saved.',
    imageReplace: 'Replace image',
    imageAdd: 'Add image',
    imageRemove: 'Remove image',
    imagePresent: '1 image',
    noImage: 'No image',
    sensory: 'Age guidance describes flavor and aroma, not safety or edibility.',
    weight: 'Grams',
    price: 'Price €',
    purchaseDate: 'Purchase date',
    roastDate: 'Roast date',
    openedDate: 'Opened on',
    stock: 'In stock',
    description: 'Description',
    productUrl: 'Product link',
    roaster: 'Roaster',
    name: 'Name',
    actionSaveError: 'Changes could not be saved.',
    deleteBeanTitle: 'Delete Coffee Bean?',
    deleteBeanConfirm: 'Delete permanently',
    removeImageTitle: 'Remove product image?',
    removeImageText: 'The current image will be removed from this Coffee Bean.',
    removeImageConfirm: 'Remove image',
    deleteBagTitle: 'Delete Coffee Bag?',
    deleteBagText: 'The recorded purchase and roast data for this bag will be lost.',
    deleteBagConfirm: 'Delete Coffee Bag',
    discardTitle: 'Discard changes?',
    discardText: 'Your unsaved input will be lost.',
    discardConfirm: 'Discard changes',
    duplicateWarningTitle: 'Possible duplicate',
    duplicateWarningFallback: 'A similar Coffee Bean already exists.',
    saveAnyway: 'Save anyway',
  },
} as const;

@Component({
  imports: [RouterLink, ReactiveFormsModule],
  selector: 'app-coffee-bean-detail-page',
  styleUrls: ['./coffee-bean-detail-page.scss', './coffee-bean-confirmation-dialog.scss'],
  templateUrl: './coffee-bean-detail-page.html',
})
export class CoffeeBeanDetailPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(CoffeeBeansApi);
  private readonly formBuilder = inject(FormBuilder);
  private readonly router = inject(Router);
  readonly languages = inject(LanguageService);

  readonly language = this.languages.current;
  readonly copy = computed(() => COPY[this.language()]);

  readonly coffeeBean = signal<CoffeeBean | null>(null);
  readonly state = signal<'loading' | 'ready' | 'not-found' | 'error'>('loading');
  readonly editingBagId = signal<string | null>(null);
  readonly bagFormOpen = signal(false);
  readonly bagError = signal(false);
  readonly profileFormOpen = signal(false);
  readonly profileError = signal(false);
  readonly success = signal<'created' | 'updated' | null>(
    history.state?.message === 'created' ? 'created' : null,
  );
  readonly pendingImage = signal<File | null>(null);
  readonly imagePreview = signal<string | null>(null);
  readonly imageFailed = signal(false);
  readonly confirmation = signal<ConfirmationDialog | null>(null);
  readonly roastLevels: readonly RoastLevel[] = [
    'Unknown',
    'Light',
    'MediumLight',
    'Medium',
    'MediumDark',
    'Dark',
  ];
  readonly profileForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(120)]],
    roaster: ['', [Validators.required, Validators.maxLength(120)]],
    origin: ['', Validators.maxLength(240)],
    roastLevel: ['Unknown' as RoastLevel, Validators.required],
    description: ['', Validators.maxLength(1000)],
    productUrl: ['', [Validators.maxLength(2048), Validators.pattern(/^https?:\/\/[^\s]+$/i)]],
  });
  readonly bagForm = this.formBuilder.nonNullable.group({
    purchasedOn: ['', Validators.required],
    roastedOn: '',
    openedOn: '',
    initialWeightGrams: [250, [Validators.required, Validators.min(1)]],
    pricePaid: [0, [Validators.required, Validators.min(0)]],
    isInStock: true,
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.state.set('not-found');
      return;
    }

    this.api.get(id).subscribe({
      next: (coffeeBean) => {
        this.coffeeBean.set(coffeeBean);
        this.state.set('ready');
      },
      error: (error: { status?: number }) =>
        this.state.set(error.status === 404 ? 'not-found' : 'error'),
    });
  }

  editProfile(): void {
    const bean = this.coffeeBean();
    if (!bean) return;
    this.profileForm.reset({
      name: bean.name,
      roaster: bean.roaster,
      origin: bean.origin ?? '',
      roastLevel: bean.roastLevel,
      description: bean.description ?? '',
      productUrl: bean.productUrl ?? '',
    });
    this.profileFormOpen.set(true);
  }

  saveProfile(): void {
    const bean = this.coffeeBean();
    if (!bean || this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }
    const value = this.profileForm.getRawValue();
    this.api
      .update(bean.id, {
        ...value,
        name: value.name.trim(),
        roaster: value.roaster.trim(),
        origin: value.origin.trim() || null,
        description: value.description.trim() || null,
        productUrl: value.productUrl.trim() || null,
      })
      .subscribe({
        next: (updated) => {
          this.coffeeBean.set(updated);
          this.profileFormOpen.set(false);
          this.profileForm.markAsPristine();
          this.success.set('updated');
        },
        error: async (error: { status?: number; error?: { detail?: string } }) => {
          if (error.status === 409) {
            const shouldSave = await this.requestConfirmation({
              confirmLabel: this.copy().saveAnyway,
              message: error.error?.detail ?? this.copy().duplicateWarningFallback,
              title: this.copy().duplicateWarningTitle,
              tone: 'warning',
            });
            if (!shouldSave) return;
            this.profileForm.markAsDirty();
            this.api
              .update(bean.id, {
                ...value,
                name: value.name.trim(),
                roaster: value.roaster.trim(),
                origin: value.origin.trim() || null,
                description: value.description.trim() || null,
                productUrl: value.productUrl.trim() || null,
                allowDuplicate: true,
              })
              .subscribe({
                next: (updated) => {
                  this.coffeeBean.set(updated);
                  this.profileFormOpen.set(false);
                  this.profileForm.markAsPristine();
                },
                error: () => this.profileError.set(true),
              });
            return;
          }
          this.profileError.set(true);
        },
      });
  }

  async deleteCoffeeBean(): Promise<void> {
    const bean = this.coffeeBean();
    if (!bean) return;
    const confirmed = await this.requestConfirmation({
      confirmLabel: this.copy().deleteBeanConfirm,
      message:
        this.language() === 'de'
          ? `„${bean.name}“ und ${bean.bagCount} zugehörige Coffee Bag(s) werden dauerhaft gelöscht.`
          : `“${bean.name}” and ${bean.bagCount} associated Coffee Bag(s) will be permanently deleted.`,
      title: this.copy().deleteBeanTitle,
      tone: 'danger',
    });
    if (!confirmed) return;
    this.api.delete(bean.id).subscribe({
      next: () => void this.router.navigate(['/coffee-beans'], { state: { message: 'deleted' } }),
      error: () => this.profileError.set(true),
    });
  }

  selectReplacementImage(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.item(0);
    if (
      !file ||
      !['image/jpeg', 'image/png', 'image/webp'].includes(file.type) ||
      file.size > 5 * 1024 * 1024
    ) {
      this.profileError.set(true);
      return;
    }
    const previous = this.imagePreview();
    if (previous) URL.revokeObjectURL(previous);
    this.pendingImage.set(file);
    this.imagePreview.set(URL.createObjectURL(file));
  }

  saveReplacementImage(): void {
    const bean = this.coffeeBean();
    const file = this.pendingImage();
    if (!bean || !file) return;
    this.api.replaceImage(bean.id, file).subscribe({
      next: (updated) => {
        this.coffeeBean.set(updated);
        const preview = this.imagePreview();
        if (preview) URL.revokeObjectURL(preview);
        this.pendingImage.set(null);
        this.imagePreview.set(null);
        this.success.set('updated');
      },
      error: () => this.profileError.set(true),
    });
  }

  async removeImage(): Promise<void> {
    const bean = this.coffeeBean();
    if (!bean) return;
    const confirmed = await this.requestConfirmation({
      confirmLabel: this.copy().removeImageConfirm,
      message: this.copy().removeImageText,
      title: this.copy().removeImageTitle,
      tone: 'danger',
    });
    if (!confirmed) return;
    this.api.removeImage(bean.id).subscribe({
      next: (updated) => this.coffeeBean.set(updated),
      error: () => this.profileError.set(true),
    });
  }

  canDeactivate(): boolean | Promise<boolean> {
    const hasUnsavedChanges =
      (this.profileFormOpen() && this.profileForm.dirty) ||
      (this.bagFormOpen() && this.bagForm.dirty) ||
      Boolean(this.pendingImage());
    if (!hasUnsavedChanges) return true;
    return this.requestConfirmation({
      confirmLabel: this.copy().discardConfirm,
      message: this.copy().discardText,
      title: this.copy().discardTitle,
      tone: 'warning',
    });
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

  @HostListener('window:beforeunload', ['$event'])
  warnBeforeUnload(event: BeforeUnloadEvent): void {
    if (
      (this.profileFormOpen() && this.profileForm.dirty) ||
      (this.bagFormOpen() && this.bagForm.dirty) ||
      this.pendingImage()
    ) {
      event.preventDefault();
    }
  }

  @HostListener('document:keydown.escape')
  cancelConfirmation(): void {
    this.resolveConfirmation(false);
  }

  addBag(): void {
    this.editingBagId.set(null);
    this.bagForm.reset({ initialWeightGrams: 250, pricePaid: 0, isInStock: true });
    this.bagFormOpen.set(true);
  }

  editBag(bag: CoffeeBag): void {
    this.editingBagId.set(bag.id);
    this.bagForm.reset({
      purchasedOn: bag.purchasedOn,
      roastedOn: bag.roastedOn ?? '',
      openedOn: bag.openedOn ?? '',
      initialWeightGrams: bag.initialWeightGrams,
      pricePaid: bag.pricePaid,
      isInStock: bag.isInStock,
    });
    this.bagFormOpen.set(true);
  }

  saveBag(): void {
    const bean = this.coffeeBean();
    if (!bean || this.bagForm.invalid) {
      this.bagForm.markAllAsTouched();
      return;
    }
    const value = this.bagForm.getRawValue();
    const today = new Date().toISOString().slice(0, 10);
    if (
      value.purchasedOn > today ||
      value.roastedOn > today ||
      value.openedOn > today ||
      (value.openedOn && value.roastedOn && value.openedOn < value.roastedOn)
    ) {
      this.bagError.set(true);
      return;
    }
    const input: CoffeeBagInput = {
      purchasedOn: value.purchasedOn,
      roastedOn: value.roastedOn || null,
      openedOn: value.openedOn || null,
      initialWeightGrams: value.initialWeightGrams,
      pricePaid: value.pricePaid,
      isInStock: value.isInStock,
    };
    const editingId = this.editingBagId();
    const request = editingId
      ? this.api.updateBag(bean.id, editingId, input)
      : this.api.createBag(bean.id, input);
    request.subscribe({
      next: (updated) => {
        this.coffeeBean.set(updated);
        this.bagFormOpen.set(false);
        this.bagError.set(false);
      },
      error: () => this.bagError.set(true),
    });
  }

  toggleStock(bag: CoffeeBag): void {
    this.runQuickAction((bean) => this.api.setBagStock(bean.id, bag.id, !bag.isInStock));
  }

  openToday(bag: CoffeeBag): void {
    this.runQuickAction((bean) => this.api.openBag(bean.id, bag.id));
  }

  async deleteBag(bag: CoffeeBag): Promise<void> {
    const bean = this.coffeeBean();
    if (!bean) return;
    const confirmed = await this.requestConfirmation({
      confirmLabel: this.copy().deleteBagConfirm,
      message: this.copy().deleteBagText,
      title: this.copy().deleteBagTitle,
      tone: 'danger',
    });
    if (!confirmed) return;
    this.api.deleteBag(bean.id, bag.id).subscribe({
      next: (updated) => this.coffeeBean.set(updated),
      error: () => this.bagError.set(true),
    });
  }

  private runQuickAction(
    request: (bean: CoffeeBean) => ReturnType<CoffeeBeansApi['openBag']>,
  ): void {
    const previous = this.coffeeBean();
    if (!previous) return;
    this.bagError.set(false);
    request(previous).subscribe({
      next: (updated) => this.coffeeBean.set(updated),
      error: () => {
        this.coffeeBean.set(previous);
        this.bagError.set(true);
      },
    });
  }

  confirmAction(): void {
    this.resolveConfirmation(true);
  }

  private requestConfirmation(dialog: Omit<ConfirmationDialog, 'resolve'>): Promise<boolean> {
    this.resolveConfirmation(false);
    return new Promise((resolve) => this.confirmation.set({ ...dialog, resolve }));
  }

  private resolveConfirmation(confirmed: boolean): void {
    const dialog = this.confirmation();
    if (!dialog) return;
    this.confirmation.set(null);
    dialog.resolve(confirmed);
  }
}
