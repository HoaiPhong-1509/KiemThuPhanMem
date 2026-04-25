import { test, expect } from '@playwright/test';

const SITES = ['https://vnexpress.net/', 'https://zingnews.vn/'];

for (const url of SITES) {
  test(`advanced bug scan: ${url}`, async ({ page, request }) => {
    const consoleErrors: string[] = [];
    const pageErrors: string[] = [];
    const badResponses: string[] = [];

    page.on('console', msg => {
      if (msg.type() === 'error') consoleErrors.push(msg.text());
    });

    page.on('pageerror', err => pageErrors.push(String(err)));

    page.on('response', res => {
      if (res.status() >= 400) badResponses.push(`${res.status()} ${res.url()}`);
    });

    const main = await page.goto(url, { waitUntil: 'domcontentloaded', timeout: 60000 });
    await page.waitForTimeout(5000);

    // 1) Chụp bằng chứng luôn
    const safe = url.replace(/^https?:\/\//, '').replace(/[^\w.-]+/g, '_');
    await page.screenshot({ path: `artifacts/${safe}_full.png`, fullPage: true });

    // 2) Kiểm tra ảnh hỏng (naturalWidth = 0)
    const brokenImages = await page.$$eval('img', imgs =>
      imgs
        .filter(img => img.complete && (img as HTMLImageElement).naturalWidth === 0)
        .map(img => (img as HTMLImageElement).src)
    );

    // 3) Kiểm tra 20 link đầu (tránh quá nặng)
    const links = await page.$$eval('a[href]', as =>
      as.map(a => (a as HTMLAnchorElement).href).filter(Boolean)
    );
    const uniqueLinks = [...new Set(links)]
      .filter(h => h.startsWith('http'))
      .slice(0, 20);

    const brokenLinks: string[] = [];
    for (const link of uniqueLinks) {
      try {
        const r = await request.get(link, { timeout: 15000 });
        if (r.status() >= 400) brokenLinks.push(`${r.status()} ${link}`);
      } catch {
        brokenLinks.push(`ERR ${link}`);
      }
    }

    // 4) Assert
    expect(main, `Không mở được ${url}`).not.toBeNull();
    expect(main!.status(), `Main document lỗi ${url}`).toBeLessThan(400);

    expect(
      pageErrors,
      `JS runtime error tại ${url}:\n${pageErrors.slice(0, 10).join('\n')}`
    ).toHaveLength(0);

    expect(
      consoleErrors,
      `Console error tại ${url}:\n${consoleErrors.slice(0, 10).join('\n')}`
    ).toHaveLength(0);

    expect(
      brokenImages,
      `Broken images tại ${url}:\n${brokenImages.slice(0, 10).join('\n')}`
    ).toHaveLength(0);

    expect(
      brokenLinks,
      `Broken links tại ${url} (20 link đầu):\n${brokenLinks.slice(0, 20).join('\n')}`
    ).toHaveLength(0);

    // Tùy chọn: giới hạn tổng response lỗi
    expect(
      badResponses.length,
      `Có response >=400 tại ${url}. Ví dụ:\n${badResponses.slice(0, 20).join('\n')}`
    ).toBeLessThan(15);
  });
}