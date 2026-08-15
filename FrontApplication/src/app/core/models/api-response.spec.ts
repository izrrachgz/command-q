import { DetailResponse, detailPayload } from './api-response';

describe('api response helpers', () => {
  it('reads the backend detail payload property', () => {
    const response: DetailResponse<{ id: string }> = {
      success: true,
      message: 'ok',
      detail: { id: 'account-1' },
    };

    expect(detailPayload(response)?.id).toBe('account-1');
  });

  it('keeps the old data payload alias for local mocks', () => {
    const response: DetailResponse<{ id: string }> = {
      success: true,
      message: 'ok',
      data: { id: 'account-2' },
    };

    expect(detailPayload(response)?.id).toBe('account-2');
  });
});
