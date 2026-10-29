import { AxiosError, type AxiosAdapter, type AxiosResponse, type InternalAxiosRequestConfig } from 'axios'
import { afterEach, describe, expect, it } from 'vitest'
import { unwrapEnvelope } from './api-envelope'
import { ApiError } from './api-error'
import { apiClient, http } from './http-client'

const originalAdapter = http.defaults.adapter

/** Thay adapter của axios để giả lập response backend mà không cần mạng. */
function mockResponse(status: number, data: unknown) {
  const adapter: AxiosAdapter = async (config: InternalAxiosRequestConfig) => {
    const response: AxiosResponse = { data, status, statusText: '', headers: {}, config }
    if (status >= 400) throw new AxiosError('Request failed', 'ERR_BAD_RESPONSE', config, null, response)
    return response
  }
  http.defaults.adapter = adapter
}

afterEach(() => {
  http.defaults.adapter = originalAdapter
})

describe('unwrapEnvelope', () => {
  it('trả về value khi isSuccess = true', () => {
    const body = { isSuccess: true, isFailure: false, error: { code: '', message: '' }, value: { id: 1 } }
    expect(unwrapEnvelope(body)).toEqual({ id: 1 })
  })

  it('ném ApiError khi isSuccess = false', () => {
    const body = { isSuccess: false, isFailure: true, error: { code: 'Tenant.NotFound', message: 'Không thấy' } }
    expect(() => unwrapEnvelope(body, 200)).toThrowError(ApiError)
    try {
      unwrapEnvelope(body)
    } catch (error) {
      expect(error).toMatchObject({ code: 'Tenant.NotFound', message: 'Không thấy' })
    }
  })

  it('trả nguyên body nếu không phải envelope (vd /health)', () => {
    expect(unwrapEnvelope({ status: 'Healthy' })).toEqual({ status: 'Healthy' })
  })
})

describe('apiClient', () => {
  it('bóc envelope và trả về value', async () => {
    mockResponse(200, {
      isSuccess: true,
      isFailure: false,
      error: { code: '', message: '' },
      warningMessage: null,
      value: { name: 'QLDN API', version: '1.0.0' }
    })
    await expect(apiClient.get('/v1/system/info')).resolves.toEqual({ name: 'QLDN API', version: '1.0.0' })
  })

  it('chuẩn hoá ErrorResponse của backend thành ApiError', async () => {
    mockResponse(422, {
      code: 'Validation',
      type: 'https://tools.ietf.org/html/rfc4918#section-11.2',
      title: 'Validation Error',
      status: 422,
      detail: 'Dữ liệu không hợp lệ',
      errors: [{ code: 'Name', message: 'Tên là bắt buộc' }],
      traceId: 'trace-1'
    })
    await expect(apiClient.post('/v1/tenants', {})).rejects.toMatchObject({
      name: 'ApiError',
      status: 422,
      code: 'Validation',
      message: 'Dữ liệu không hợp lệ',
      details: [{ code: 'Name', message: 'Tên là bắt buộc' }],
      traceId: 'trace-1'
    })
  })

  it('lỗi mạng (không có response) → status 0 với thông điệp tiếng Việt', async () => {
    http.defaults.adapter = async (config) => {
      throw new AxiosError('Network Error', 'ERR_NETWORK', config)
    }
    await expect(apiClient.get('/v1/system/info')).rejects.toMatchObject({ status: 0, code: 'ERR_NETWORK' })
  })
})
