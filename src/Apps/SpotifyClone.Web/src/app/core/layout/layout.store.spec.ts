import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { LIBRARY_WIDTH, LayoutStore, RIGHT_PANEL_WIDTH } from './layout.store';
import { ViewportService } from './viewport.service';

describe('LayoutStore', () => {
  const viewport = {
    isMobile: signal(false),
    isBelowLaptop: signal(false),
    isBelowDesktop: signal(false),
  };

  function setup(): LayoutStore {
    localStorage.clear();
    viewport.isMobile.set(false);
    viewport.isBelowLaptop.set(false);
    viewport.isBelowDesktop.set(false);
    TestBed.configureTestingModule({ providers: [{ provide: ViewportService, useValue: viewport }] });
    const store = TestBed.inject(LayoutStore);
    TestBed.tick();
    return store;
  }

  it('starts with the library expanded and the Now Playing panel open on desktop', () => {
    const store = setup();
    expect(store.libraryCollapsed()).toBe(false);
    expect(store.libraryWidth()).toBe(LIBRARY_WIDTH.default);
    expect(store.rightPanel()).toBe('now-playing');
    expect(store.isRightPanelVisible()).toBe(true);
  });

  it('toggles between right panels and closes when the open one is toggled again', () => {
    const store = setup();
    store.toggleRightPanel('queue');
    expect(store.rightPanel()).toBe('queue');
    store.toggleRightPanel('queue');
    expect(store.rightPanel()).toBeNull();
    expect(store.isRightPanelVisible()).toBe(false);
  });

  it('clamps the library width and snaps to the collapsed rail when dragged narrow', () => {
    const store = setup();
    store.resizeLibrary(10_000);
    expect(store.libraryWidth()).toBe(LIBRARY_WIDTH.max);

    store.resizeLibrary(LIBRARY_WIDTH.collapseThreshold - 1);
    expect(store.libraryCollapsed()).toBe(true);
    expect(store.libraryWidth()).toBe(LIBRARY_WIDTH.collapsed);

    store.resizeLibrary(300);
    expect(store.libraryCollapsed()).toBe(false);
    expect(store.libraryWidth()).toBe(300);
  });

  it('clamps the right panel width', () => {
    const store = setup();
    store.resizeRightPanel(0);
    expect(store.rightPanelWidth()).toBe(RIGHT_PANEL_WIDTH.min);
    store.resizeRightPanel(10_000);
    expect(store.rightPanelWidth()).toBe(RIGHT_PANEL_WIDTH.max);
  });

  it('persists sizes to localStorage', () => {
    const store = setup();
    store.resizeLibrary(350);
    expect(JSON.parse(localStorage.getItem('stopify.layout')!)).toEqual(
      expect.objectContaining({ libraryWidth: 350 }),
    );
  });

  it('collapses the library and closes the right panel on narrow screens', () => {
    const store = setup();
    viewport.isBelowLaptop.set(true);
    viewport.isBelowDesktop.set(true);
    TestBed.tick();
    expect(store.libraryCollapsed()).toBe(true);
    expect(store.rightPanel()).toBeNull();
  });

  it('never shows the right panel on phones', () => {
    const store = setup();
    viewport.isMobile.set(true);
    store.openRightPanel('queue');
    expect(store.isRightPanelVisible()).toBe(false);
  });
});
