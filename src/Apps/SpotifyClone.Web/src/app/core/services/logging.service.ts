import { Injectable } from '@angular/core';

export type LogLevel = 'DEBUG' | 'INFO' | 'WARN' | 'ERROR';

@Injectable({ providedIn: 'root' })
export class LoggingService {
  private tag = 'Stopify';

  debug(message: string, context?: unknown): void {
    this.log('DEBUG', message, context);
  }

  info(message: string, context?: unknown): void {
    this.log('INFO', message, context);
  }

  warn(message: string, context?: unknown): void {
    this.log('WARN', message, context);
  }

  error(message: string, context?: unknown): void {
    this.log('ERROR', message, context, console.error);
  }

  private log(
    level: LogLevel,
    message: string,
    context?: unknown,
    output: typeof console.log = console.log,
  ): void {
    const timestamp = new Date().toISOString();
    const prefix = `[${timestamp}] [${this.tag}] [${level}]`;
    const formatted = context
      ? `${prefix} ${message}`: `${prefix} ${message}`;

    output(formatted);
    if (context !== undefined) {
      output(context);
    }
  }
}
