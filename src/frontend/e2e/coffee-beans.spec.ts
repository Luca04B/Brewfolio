import { expect, test } from '@playwright/test';

test('creates a Coffee Bean, records a bag, and finds it in the collection', async ({
  page,
}, testInfo) => {
  const name = `E2E Coffee ${Date.now()}-${testInfo.workerIndex}`;
  await page.goto('/coffee-beans/new');
  await page.locator('input[name="name"]').fill(name);
  await page.locator('input[name="roaster"]').fill('Brewfolio Test Roasters');
  await page.locator('input[name="includeFirstBag"]').check();
  await page.locator('input[name="purchasedOn"]').fill('2026-09-05');
  await page.locator('input[type="file"]').setInputFiles({
    name: 'coffee.png',
    mimeType: 'image/png',
    buffer: Buffer.from(
      'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=',
      'base64',
    ),
  });
  await page.locator('button[type="submit"]').click();
  await expect(page).toHaveURL(/\/coffee-beans\/[0-9a-f-]{36}$/);
  await expect(page.getByRole('heading', { level: 1 })).toContainText(name);
  await expect(page.locator('.art img')).toBeVisible();
  await expect(page.locator('.ledger article')).toHaveCount(1);

  await page
    .locator('.product-actions')
    .getByRole('button', { name: /^Edit$/ })
    .click();
  await page.locator('.profile-editor textarea').fill('Edited in the browser happy path');
  await page.locator('.profile-editor button[type="submit"]').click();
  await expect(page.getByText('Edited in the browser happy path')).toBeVisible();

  await page
    .locator('.ledger article')
    .getByRole('button', { name: /^Edit$/ })
    .click();
  await page.locator('.bag-form input[formcontrolname="pricePaid"]').fill('12.50');
  await page.locator('.bag-form button[type="submit"]').click();
  await page
    .locator('.ledger article')
    .getByRole('button', { name: /Remove from stock/ })
    .click();
  await page
    .locator('.ledger article')
    .getByRole('button', { name: /Open today/ })
    .click();
  await expect(page.locator('.ledger article')).toContainText('€12.50');

  await page.locator('.image-actions input[type="file"]').setInputFiles({
    name: 'replacement.png',
    mimeType: 'image/png',
    buffer: Buffer.from(
      'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=',
      'base64',
    ),
  });
  await page.getByRole('button', { name: /Save image/ }).click();
  await page.getByRole('button', { name: /Remove image/ }).click();
  await page.getByTestId('confirm-action').click();
  await expect(page.locator('.art img')).toHaveCount(0);

  await page.goto('/coffee-beans');
  await page.locator('input[type="search"]').fill(name);
  await expect(page.getByTestId('bean-name').filter({ hasText: name })).toHaveCount(1);
  await page.locator('a.bean-card').filter({ hasText: name }).click();
  await page
    .locator('.product-actions')
    .getByRole('button', { name: /^Delete$/ })
    .click();
  await page.getByTestId('confirm-action').click();
  await expect(page).toHaveURL(/\/coffee-beans$/);
});
