import {
  AngularNodeAppEngine,
  createNodeRequestHandler,
  isMainModule,
  writeResponseToNodeResponse,
} from '@angular/ssr/node';
import express from 'express';
import { join } from 'node:path';

const browserDistFolder = join(import.meta.dirname, '../browser');

const app = express();
const angularApp = new AngularNodeAppEngine();

/**
 * The API (/api, /hubs) and the media storage (/audio, /images) are separate services: in production a
 * reverse proxy routes those paths to them. If one reaches this server anyway, answer 404 instead of
 * rendering the app's HTML, so a missing proxy rule fails loudly rather than as "invalid JSON".
 */
app.use(['/api', '/hubs', '/audio', '/images'], (req, res) => {
  res
    .status(404)
    .type('application/problem+json')
    .send(
      JSON.stringify({
        title: 'NotFound',
        status: 404,
        detail: `${req.originalUrl} is served by the Stopify API or storage, not the web app. Route it there in the reverse proxy.`,
      }),
    );
});

/**
 * Serve static files from /browser
 */
app.use(
  express.static(browserDistFolder, {
    maxAge: '1y',
    index: false,
    redirect: false,
  }),
);

/**
 * Handle all other requests by rendering the Angular application.
 */
app.use((req, res, next) => {
  angularApp
    .handle(req)
    .then((response) =>
      response ? writeResponseToNodeResponse(response, res) : next(),
    )
    .catch(next);
});

/**
 * Start the server if this module is the main entry point, or it is ran via PM2.
 * The server listens on the port defined by the `PORT` environment variable, or defaults to 4000.
 */
if (isMainModule(import.meta.url) || process.env['pm_id']) {
  const port = process.env['PORT'] || 4000;
  app.listen(port, (error) => {
    if (error) {
      throw error;
    }

    console.log(`Node Express server listening on http://localhost:${port}`);
  });
}

/**
 * Request handler used by the Angular CLI (for dev-server and during build) or Firebase Cloud Functions.
 */
export const reqHandler = createNodeRequestHandler(app);
