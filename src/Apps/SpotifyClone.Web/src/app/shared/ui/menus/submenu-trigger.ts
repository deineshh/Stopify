import { Directive, inject } from '@angular/core';
import { CdkMenuTrigger } from '@angular/cdk/menu';

/**
 * For menu items that open a submenu ("Add to playlist ▸"): a click always opens it.
 *
 * The CDK trigger toggles on click, but hovering the item has usually opened the submenu
 * already, so the click that follows would close it again. Closing stays available with ←,
 * Escape or by moving to another item.
 */
@Directive({ selector: '[cdkMenuTriggerFor][appSubmenuTrigger]' })
export class SubmenuTrigger {
  constructor() {
    const trigger = inject(CdkMenuTrigger, { self: true });
    trigger.toggle = () => trigger.open();
  }
}
