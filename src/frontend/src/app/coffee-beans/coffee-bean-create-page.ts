import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { CoffeeBeansApi, RoastLevel } from './coffee-beans-api';
import { LanguageService } from './language.service';

const COPY = {
  de: {
    back: 'Zur Sammlung',
    eyebrow: 'Neues Produkt',
    title: 'Coffee Bean anlegen',
    intro:
      'Erfasse das Produkt einmal. Einzelne Käufe und Röstdaten verwaltest du als Coffee Bags.',
    name: 'Name',
    roaster: 'Rösterei',
    origin: 'Herkunft',
    roastLevel: 'Röstgrad',
    description: 'Kurzbeschreibung',
    productUrl: 'Produktlink',
    optional: 'Optional',
    required: 'Dieses Feld ist erforderlich.',
    invalidUrl: 'Bitte eine vollständige HTTP- oder HTTPS-Adresse eingeben.',
    save: 'Coffee Bean speichern',
    saving: 'Wird gespeichert …',
    error: 'Die Coffee Bean konnte nicht gespeichert werden. Bitte prüfe deine Angaben.',
    firstBag: 'Erste Packung erfassen',
    purchaseDate: 'Kaufdatum',
    roastDate: 'Röstdatum',
    openedDate: 'Geöffnet am',
    weight: 'Gewicht in Gramm',
    price: 'Preis in Euro',
    inStock: 'Im Vorrat',
    image: 'Produktbild',
    imageHint: 'JPEG, PNG oder WebP · maximal 5 MB',
    imageInvalid: 'Bitte wähle ein gültiges Bild bis 5 MB.',
    imageRemove: 'Bild entfernen',
  },
  en: {
    back: 'Back to collection',
    eyebrow: 'New product',
    title: 'Add Coffee Bean',
    intro:
      'Capture the product once. Individual purchases and roast dates are managed as Coffee Bags.',
    name: 'Name',
    roaster: 'Roaster',
    origin: 'Origin',
    roastLevel: 'Roast level',
    description: 'Short description',
    productUrl: 'Product link',
    optional: 'Optional',
    required: 'This field is required.',
    invalidUrl: 'Enter a complete HTTP or HTTPS address.',
    save: 'Save Coffee Bean',
    saving: 'Saving …',
    error: 'The Coffee Bean could not be saved. Please check your input.',
    firstBag: 'Record first bag',
    purchaseDate: 'Purchase date',
    roastDate: 'Roast date',
    openedDate: 'Opened on',
    weight: 'Weight in grams',
    price: 'Price in euros',
    inStock: 'In stock',
    image: 'Product image',
    imageHint: 'JPEG, PNG, or WebP · maximum 5 MB',
    imageInvalid: 'Choose a valid image up to 5 MB.',
    imageRemove: 'Remove image',
  },
} as const;

@Component({
  imports: [ReactiveFormsModule, RouterLink],
  selector: 'app-coffee-bean-create-page',
  styleUrl: './coffee-bean-create-page.scss',
  templateUrl: './coffee-bean-create-page.html',
})
export class CoffeeBeanCreatePage {
  private readonly api = inject(CoffeeBeansApi);
  private readonly router = inject(Router);
  private readonly formBuilder = inject(FormBuilder);
  private readonly languages = inject(LanguageService);

  readonly language = this.languages.current;
  readonly copy = computed(() => COPY[this.language()]);
  readonly roastLevels: readonly RoastLevel[] = [
    'Unknown',
    'Light',
    'MediumLight',
    'Medium',
    'MediumDark',
    'Dark',
  ];
  readonly saving = signal(false);
  readonly failed = signal(false);
  readonly selectedImage = signal<File | null>(null);
  readonly imagePreview = signal<string | null>(null);
  readonly imageInvalid = signal(false);
  readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(120)]],
    roaster: ['', [Validators.required, Validators.maxLength(120)]],
    origin: ['', Validators.maxLength(240)],
    roastLevel: ['Unknown' as RoastLevel, Validators.required],
    description: ['', Validators.maxLength(1_000)],
    productUrl: ['', [Validators.maxLength(2_048), Validators.pattern(/^https?:\/\/[^\s]+$/i)]],
    includeFirstBag: false,
    purchasedOn: '',
    roastedOn: '',
    openedOn: '',
    initialWeightGrams: 250,
    pricePaid: 0,
    isInStock: true,
  });

  submit(allowDuplicate = false): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request = {
      name: value.name.trim(),
      roaster: value.roaster.trim(),
      origin: optional(value.origin),
      roastLevel: value.roastLevel,
      description: optional(value.description),
      productUrl: optional(value.productUrl),
      firstBag: value.includeFirstBag
        ? {
            purchasedOn: value.purchasedOn,
            roastedOn: optional(value.roastedOn),
            openedOn: optional(value.openedOn),
            initialWeightGrams: value.initialWeightGrams,
            pricePaid: value.pricePaid,
            isInStock: value.isInStock,
          }
        : null,
      allowDuplicate,
    };
    const today = new Date().toISOString().slice(0, 10);
    if (
      value.includeFirstBag &&
      (!value.purchasedOn ||
        value.purchasedOn > today ||
        value.roastedOn > today ||
        value.openedOn > today ||
        (value.openedOn && value.roastedOn && value.openedOn < value.roastedOn) ||
        value.initialWeightGrams <= 0 ||
        value.pricePaid < 0)
    ) {
      this.failed.set(true);
      return;
    }
    if (!request.name || !request.roaster) {
      if (!request.name) this.form.controls.name.setErrors({ required: true });
      if (!request.roaster) this.form.controls.roaster.setErrors({ required: true });
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.failed.set(false);
    this.api
      .create(request, this.selectedImage() ?? undefined)
      .pipe(finalize(() => this.saving.set(false)))
      .subscribe({
        next: (coffeeBean) => {
          this.form.markAsPristine();
          void this.router.navigate(['/coffee-beans', coffeeBean.id], {
            state: { message: 'created' },
          });
        },
        error: (error: { status?: number; error?: { detail?: string } }) => {
          if (
            error.status === 409 &&
            globalThis.confirm(
              `${error.error?.detail ?? 'Possible duplicate.'}\n\n${this.language() === 'de' ? 'Trotzdem anlegen?' : 'Create it anyway?'}`,
            )
          ) {
            this.submit(true);
            return;
          }
          this.failed.set(true);
        },
      });
  }

  toggleLanguage(): void {
    this.languages.toggle();
  }

  canDeactivate(): boolean {
    return (
      !this.form.dirty || this.saving() || globalThis.confirm('Ungespeicherte Eingaben verwerfen?')
    );
  }

  selectImage(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.item(0) ?? null;
    const accepted =
      file &&
      ['image/jpeg', 'image/png', 'image/webp'].includes(file.type) &&
      file.size <= 5 * 1024 * 1024;
    this.imageInvalid.set(Boolean(file && !accepted));
    this.selectedImage.set(accepted ? file : null);
    const previous = this.imagePreview();
    if (previous) URL.revokeObjectURL(previous);
    this.imagePreview.set(accepted && file ? URL.createObjectURL(file) : null);
  }

  clearImage(input: HTMLInputElement): void {
    const preview = this.imagePreview();
    if (preview) URL.revokeObjectURL(preview);
    this.selectedImage.set(null);
    this.imagePreview.set(null);
    this.imageInvalid.set(false);
    input.value = '';
  }
}

function optional(value: string): string | null {
  const normalized = value.trim();
  return normalized.length === 0 ? null : normalized;
}
