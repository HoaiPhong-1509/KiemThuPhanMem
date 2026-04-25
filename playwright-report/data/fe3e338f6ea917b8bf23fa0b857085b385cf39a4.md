# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: news-bug-scan.spec.ts >> advanced bug scan: https://vnexpress.net/
- Location: tests\news-bug-scan.spec.ts:6:7

# Error details

```
Error: page.goto: net::ERR_CONNECTION_TIMED_OUT at https://vnexpress.net/
Call log:
  - navigating to "https://vnexpress.net/", waiting until "domcontentloaded"

```

# Page snapshot

```yaml
- generic [ref=e2]:
  - generic [ref=e3]:
    - generic [ref=e6]:
      - heading "Hmmm… can't reach this page" [level=1] [ref=e7]
      - paragraph [ref=e8]:
        - strong [ref=e9]: vnexpress.net
        - text: took too long to respond
      - generic [ref=e10]:
        - paragraph [ref=e11]: "Try:"
        - list [ref=e12]:
          - listitem [ref=e13]: •Checking the connection
          - listitem [ref=e14]:
            - text: •
            - link "Checking the proxy and the firewall" [ref=e15] [cursor=pointer]:
              - /url: "#buttons"
      - generic [ref=e16]: ERR_CONNECTION_TIMED_OUT
    - button "Refresh" [ref=e19] [cursor=pointer]
  - generic [ref=e23]: Microsoft Edge
```

# Test source

```ts
  1  | import { test, expect } from '@playwright/test';
  2  | 
  3  | const SITES = ['https://vnexpress.net/', 'https://zingnews.vn/'];
  4  | 
  5  | for (const url of SITES) {
  6  |   test(`advanced bug scan: ${url}`, async ({ page, request }) => {
  7  |     const consoleErrors: string[] = [];
  8  |     const pageErrors: string[] = [];
  9  |     const badResponses: string[] = [];
  10 | 
  11 |     page.on('console', msg => {
  12 |       if (msg.type() === 'error') consoleErrors.push(msg.text());
  13 |     });
  14 | 
  15 |     page.on('pageerror', err => pageErrors.push(String(err)));
  16 | 
  17 |     page.on('response', res => {
  18 |       if (res.status() >= 400) badResponses.push(`${res.status()} ${res.url()}`);
  19 |     });
  20 | 
> 21 |     const main = await page.goto(url, { waitUntil: 'domcontentloaded', timeout: 60000 });
     |                             ^ Error: page.goto: net::ERR_CONNECTION_TIMED_OUT at https://vnexpress.net/
  22 |     await page.waitForTimeout(5000);
  23 | 
  24 |     // 1) Chụp bằng chứng luôn
  25 |     const safe = url.replace(/^https?:\/\//, '').replace(/[^\w.-]+/g, '_');
  26 |     await page.screenshot({ path: `artifacts/${safe}_full.png`, fullPage: true });
  27 | 
  28 |     // 2) Kiểm tra ảnh hỏng (naturalWidth = 0)
  29 |     const brokenImages = await page.$$eval('img', imgs =>
  30 |       imgs
  31 |         .filter(img => img.complete && (img as HTMLImageElement).naturalWidth === 0)
  32 |         .map(img => (img as HTMLImageElement).src)
  33 |     );
  34 | 
  35 |     // 3) Kiểm tra 20 link đầu (tránh quá nặng)
  36 |     const links = await page.$$eval('a[href]', as =>
  37 |       as.map(a => (a as HTMLAnchorElement).href).filter(Boolean)
  38 |     );
  39 |     const uniqueLinks = [...new Set(links)]
  40 |       .filter(h => h.startsWith('http'))
  41 |       .slice(0, 20);
  42 | 
  43 |     const brokenLinks: string[] = [];
  44 |     for (const link of uniqueLinks) {
  45 |       try {
  46 |         const r = await request.get(link, { timeout: 15000 });
  47 |         if (r.status() >= 400) brokenLinks.push(`${r.status()} ${link}`);
  48 |       } catch {
  49 |         brokenLinks.push(`ERR ${link}`);
  50 |       }
  51 |     }
  52 | 
  53 |     // 4) Assert
  54 |     expect(main, `Không mở được ${url}`).not.toBeNull();
  55 |     expect(main!.status(), `Main document lỗi ${url}`).toBeLessThan(400);
  56 | 
  57 |     expect(
  58 |       pageErrors,
  59 |       `JS runtime error tại ${url}:\n${pageErrors.slice(0, 10).join('\n')}`
  60 |     ).toHaveLength(0);
  61 | 
  62 |     expect(
  63 |       consoleErrors,
  64 |       `Console error tại ${url}:\n${consoleErrors.slice(0, 10).join('\n')}`
  65 |     ).toHaveLength(0);
  66 | 
  67 |     expect(
  68 |       brokenImages,
  69 |       `Broken images tại ${url}:\n${brokenImages.slice(0, 10).join('\n')}`
  70 |     ).toHaveLength(0);
  71 | 
  72 |     expect(
  73 |       brokenLinks,
  74 |       `Broken links tại ${url} (20 link đầu):\n${brokenLinks.slice(0, 20).join('\n')}`
  75 |     ).toHaveLength(0);
  76 | 
  77 |     // Tùy chọn: giới hạn tổng response lỗi
  78 |     expect(
  79 |       badResponses.length,
  80 |       `Có response >=400 tại ${url}. Ví dụ:\n${badResponses.slice(0, 20).join('\n')}`
  81 |     ).toBeLessThan(15);
  82 |   });
  83 | }
```