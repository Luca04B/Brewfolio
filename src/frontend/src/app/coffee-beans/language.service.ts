import { Injectable, signal } from '@angular/core';

export type Language = 'de' | 'en';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  readonly current = signal<Language>(detectLanguage());

  constructor() {
    if (globalThis.document) globalThis.document.documentElement.lang = this.current();
  }

  toggle(): void {
    const language = this.current() === 'de' ? 'en' : 'de';
    this.current.set(language);
    globalThis.localStorage?.setItem('brewfolio-language', language);
    if (globalThis.document) globalThis.document.documentElement.lang = language;
  }
}

function detectLanguage(): Language {
  const stored = globalThis.localStorage?.getItem('brewfolio-language');
  if (stored === 'de' || stored === 'en') return stored;
  return globalThis.navigator?.language.toLowerCase().startsWith('de') ? 'de' : 'en';
}
