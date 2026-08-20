import {
  virtualTableBufferPixels,
  virtualTableViewportHeight,
  VirtualTableRowHeight,
} from './virtual-table';

describe('virtual table sizing', () => {
  it('uses the feature items-per-page value as the default viewport and buffer size', () => {
    const itemsPerPage = 25;

    expect(virtualTableViewportHeight(100, itemsPerPage)).toBe(
      itemsPerPage * VirtualTableRowHeight,
    );
    expect(virtualTableBufferPixels(itemsPerPage)).toBe(itemsPerPage * VirtualTableRowHeight);
  });

  it('does not reserve empty rows when the current page is shorter', () => {
    expect(virtualTableViewportHeight(3, 25)).toBe(3 * VirtualTableRowHeight);
  });
});
