import { expect, test } from '@playwright/test';

const ONE_PIXEL_PNG = Buffer.from(
  'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=',
  'base64',
);

test('removes a selected image before creating', async ({ page }) => {
  await page.goto('/coffee-beans/new');
  await page.locator('input[type="file"]').setInputFiles({
    name: 'coffee.png',
    mimeType: 'image/png',
    buffer: ONE_PIXEL_PNG,
  });

  await expect(page.locator('.image-preview')).toBeVisible();
  await page.getByRole('button', { name: /Remove image/ }).click();

  await expect(page.locator('.image-preview')).toHaveCount(0);
  await expect(page.locator('input[type="file"]')).toHaveJSProperty('files.length', 0);
});

test('creates a Coffee Bean with an allowed 5 MiB image through the frontend proxy', async ({
  page,
}) => {
  const name = `Image upload ${Date.now()}`;
  const image = Buffer.concat([
    ONE_PIXEL_PNG,
    Buffer.alloc(5 * 1024 * 1024 - ONE_PIXEL_PNG.length),
  ]);

  await page.goto('/coffee-beans/new');
  await page.locator('input[name="name"]').fill(name);
  await page.locator('input[name="roaster"]').fill('Brewfolio Test Roasters');
  await page.locator('input[type="file"]').setInputFiles({
    name: 'coffee.png',
    mimeType: 'image/png',
    buffer: image,
  });
  await expect(page.locator('.image-picker .validation')).toHaveCount(0);
  const createResponse = page.waitForResponse(
    (response) =>
      response.request().method() === 'POST' &&
      new URL(response.url()).pathname === '/api/coffee-beans',
  );
  await page.locator('button[type="submit"]').click();
  const response = await createResponse;

  expect(response.status(), await response.text()).toBe(201);
  await expect(page).toHaveURL(/\/coffee-beans\/[0-9a-f-]{36}$/);
  await expect(page.locator('.art img')).toBeVisible();
  const id = page.url().split('/').at(-1);
  if (id) await page.request.delete(`/api/coffee-beans/${id}`);
});
