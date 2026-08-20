export const VirtualTableRowHeight = 64;

export const virtualTableViewportHeight = (itemCount: number, itemsPerPage: number): number => {
  const rowCount = Math.min(Math.max(itemCount, 1), Math.max(itemsPerPage, 1));
  return rowCount * VirtualTableRowHeight;
};

export const virtualTableBufferPixels = (itemsPerPage: number): number => {
  return Math.max(itemsPerPage, 1) * VirtualTableRowHeight;
};

export const trackByEntityId = (_index: number, item: { readonly id: unknown }): unknown => {
  return item.id;
};
