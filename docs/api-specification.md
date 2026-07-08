# SafeFlow-AI — API Specification

> **Versiyon:** 1.1  
> **Tarih:** 2026-07-07  
> **Durum:** Taslak — Onay Bekliyor  
> **Format:** OpenAPI 3.1 uyumlu REST API tasarımı  
> **Base URL:** `https://api.safeflow.ai/v1`

> [!IMPORTANT]
> **SafeFlow-AI**, yalnızca bir İSG yazılımı değildir. İş Sağlığı ve Güvenliği süreçlerini
> dijitalleştiren, otomatikleştiren ve yapay zekâ ile karar desteği sağlayan **modüler bir
> operasyon platformudur**. Bu API sözleşmesi, platformun tüm modüllerini (eğitim, risk
> değerlendirme, saha denetimi, KKD yönetimi, olay yönetimi, mevzuat uyumluluğu) kapsayacak
> şekilde genişletilebilir tasarlanmıştır.

---

## 1. API Tasarım Prensipleri

| Prensip | Detay |
|---------|-------|
| **Versiyonlama** | URL-based: `/v1/`, `/v2/` |
| **Naming** | kebab-case resource isimleri: `/training-sessions`, `/risk-assessments` |
| **HTTP Methods** | Semantik doğruluk: GET (okuma), POST (oluşturma), PUT (güncelleme), PATCH (kısmi güncelleme), DELETE (silme) |
| **Response Format** | JSON (application/json), tutarlı envelope |
| **Pagination** | Cursor-based veya offset-based (kaynak tipine göre) |
| **Filtering** | Query parameter: `?status=active&departmentId=xxx` |
| **Sorting** | `?sortBy=createdAt&sortOrder=desc` |
| **Error Format** | RFC 7807 Problem Details |
| **Authentication** | Bearer JWT Token |
| **Multi-Tenant** | JWT claim'den tenant isolation |
| **Idempotency** | POST istekleri için `X-Idempotency-Key` header desteği |
| **Rate Limiting** | `X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset` header'ları |
| **Correlation** | Tüm response'larda `X-Correlation-Id` header |

---

## 2. Ortak Response Envelope'ları

### 2.1 Başarılı Tekil Response

```json
{
  "data": { ... },
  "meta": {
    "timestamp": "2026-07-07T10:00:00Z",
    "correlationId": "550e8400-e29b-41d4-a716-446655440000"
  }
}
```

### 2.2 Başarılı Liste Response (Paginated)

```json
{
  "data": [ ... ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8,
    "hasNext": true,
    "hasPrevious": false
  },
  "meta": {
    "timestamp": "2026-07-07T10:00:00Z",
    "correlationId": "550e8400-e29b-41d4-a716-446655440000"
  }
}
```

### 2.3 Error Response (RFC 7807)

```json
{
  "type": "https://api.safeflow.ai/errors/validation-error",
  "title": "Validation Error",
  "status": 422,
  "detail": "One or more validation errors occurred.",
  "instance": "/v1/trainings",
  "errors": {
    "title": ["Title is required."],
    "maxParticipants": ["Must be greater than 0."]
  },
  "correlationId": "550e8400-e29b-41d4-a716-446655440000"
}
```

### 2.4 HTTP Status Code Politikası

| Code | Kullanım |
|------|----------|
| `200 OK` | Başarılı GET, PUT, PATCH |
| `201 Created` | Başarılı POST (Location header ile) |
| `204 No Content` | Başarılı DELETE |
| `400 Bad Request` | Geçersiz istek formatı |
| `401 Unauthorized` | Kimlik doğrulama hatası |
| `403 Forbidden` | Yetki hatası |
| `404 Not Found` | Kaynak bulunamadı |
| `409 Conflict` | Çakışma (duplicate, state conflict) |
| `422 Unprocessable Entity` | Doğrulama hatası |
| `429 Too Many Requests` | Rate limit aşıldı |
| `500 Internal Server Error` | Sunucu hatası |

---

## 3. Authentication & Authorization API

### 3.1 POST `/v1/auth/register`
Yeni kullanıcı kaydı oluşturur.

**Request:**
```json
{
  "email": "ahmet.yilmaz@company.com",
  "password": "SecureP@ss123",
  "firstName": "Ahmet",
  "lastName": "Yılmaz",
  "phoneNumber": "+905551234567",
  "companyName": "ABC Sanayi A.Ş.",
  "companyTaxNumber": "1234567890"
}
```

**Response: `201 Created`**
```json
{
  "data": {
    "userId": "a1b2c3d4-...",
    "email": "ahmet.yilmaz@company.com",
    "tenantId": "t1t2t3t4-...",
    "status": "Pending"
  }
}
```

---

### 3.2 POST `/v1/auth/login`
Kullanıcı girişi. JWT Access Token + Refresh Token döner.

**Request:**
```json
{
  "email": "ahmet.yilmaz@company.com",
  "password": "SecureP@ss123"
}
```

**Response: `200 OK`**
```json
{
  "data": {
    "accessToken": "eyJhbGciOiJSUzI1NiIs...",
    "refreshToken": "dGhpcyBpcyBhIHJlZnJl...",
    "expiresIn": 900,
    "tokenType": "Bearer",
    "user": {
      "id": "a1b2c3d4-...",
      "email": "ahmet.yilmaz@company.com",
      "fullName": "Ahmet Yılmaz",
      "roles": ["ISGUzmani"],
      "permissions": ["trainings.view", "trainings.create", "certificates.view"],
      "tenantId": "t1t2t3t4-..."
    }
  }
}
```

> [!NOTE]
> Access Token süresi: 15 dakika. Refresh Token süresi: 7 gün.  
> Refresh Token HttpOnly Secure cookie olarak da gönderilebilir.

---

### 3.3 POST `/v1/auth/refresh-token`
Token yenileme. Token Rotation uygulanır — her refresh işleminde eski token iptal edilir.

**Request:**
```json
{
  "refreshToken": "dGhpcyBpcyBhIHJlZnJl..."
}
```

**Response: `200 OK`**
```json
{
  "data": {
    "accessToken": "eyJhbGciOiJSUzI1NiIs...(yeni)",
    "refreshToken": "bmV3IHJlZnJlc2ggdG9r...(yeni)",
    "expiresIn": 900
  }
}
```

---

### 3.4 POST `/v1/auth/revoke-token`
Refresh token'ı iptal eder (logout).

**Request:**
```json
{
  "refreshToken": "dGhpcyBpcyBhIHJlZnJl..."
}
```

**Response: `204 No Content`**

---

### 3.5 POST `/v1/auth/forgot-password`

**Request:**
```json
{
  "email": "ahmet.yilmaz@company.com"
}
```

**Response: `200 OK`** (güvenlik nedeniyle her zaman 200)

---

### 3.6 POST `/v1/auth/reset-password`

**Request:**
```json
{
  "token": "reset-token-...",
  "newPassword": "NewSecureP@ss456"
}
```

**Response: `200 OK`**

---

### 3.7 GET `/v1/auth/me`
🔒 **Requires:** Authenticated

Mevcut kullanıcı bilgilerini döner.

**Response: `200 OK`**
```json
{
  "data": {
    "id": "a1b2c3d4-...",
    "email": "ahmet.yilmaz@company.com",
    "fullName": "Ahmet Yılmaz",
    "phoneNumber": "+905551234567",
    "roles": ["ISGUzmani"],
    "permissions": ["trainings.view", "trainings.create"],
    "tenantId": "t1t2t3t4-...",
    "tenantName": "ABC Sanayi A.Ş.",
    "lastLoginAt": "2026-07-07T09:00:00Z"
  }
}
```

---

## 4. User Management API

### 4.1 GET `/v1/users`
🔒 **Requires:** `users.view`

**Query Parameters:**
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| `page` | int | 1 | Sayfa numarası |
| `pageSize` | int | 20 | Sayfa boyutu (max: 100) |
| `search` | string | — | İsim veya email araması |
| `status` | string | — | `Active`, `Inactive`, `Locked`, `Pending` |
| `roleId` | guid | — | Role göre filtreleme |
| `sortBy` | string | `createdAt` | Sıralama alanı |
| `sortOrder` | string | `desc` | `asc` veya `desc` |

**Response: `200 OK`**
```json
{
  "data": [
    {
      "id": "a1b2c3d4-...",
      "email": "ahmet.yilmaz@company.com",
      "fullName": "Ahmet Yılmaz",
      "status": "Active",
      "roles": ["ISGUzmani"],
      "lastLoginAt": "2026-07-07T09:00:00Z",
      "createdAt": "2026-01-15T10:00:00Z"
    }
  ],
  "pagination": { "page": 1, "pageSize": 20, "totalCount": 45 }
}
```

---

### 4.2 GET `/v1/users/{id}`
🔒 **Requires:** `users.view`

### 4.3 POST `/v1/users`
🔒 **Requires:** `users.create`

**Request:**
```json
{
  "email": "mehmet.kaya@company.com",
  "firstName": "Mehmet",
  "lastName": "Kaya",
  "phoneNumber": "+905559876543",
  "roleIds": ["role-id-1", "role-id-2"]
}
```

**Response: `201 Created`**
- `Location: /v1/users/new-user-id`

### 4.4 PUT `/v1/users/{id}`
🔒 **Requires:** `users.update`

### 4.5 PATCH `/v1/users/{id}/status`
🔒 **Requires:** `users.update`

**Request:**
```json
{
  "status": "Active",
  "reason": "Email verified"
}
```

### 4.6 POST `/v1/users/{id}/roles`
🔒 **Requires:** `users.assign-role`

**Request:**
```json
{
  "roleIds": ["role-id-1"]
}
```

### 4.7 DELETE `/v1/users/{id}/roles/{roleId}`
🔒 **Requires:** `users.assign-role`

---

## 5. Training API

### 5.1 GET `/v1/trainings`
🔒 **Requires:** `trainings.view`

**Query Parameters:**
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| `page` | int | 1 | Sayfa |
| `pageSize` | int | 20 | Boyut |
| `search` | string | — | Başlık araması |
| `status` | string | — | `Draft`, `Published`, `InProgress`, `Completed`, `Cancelled` |
| `type` | string | — | `Mandatory`, `Optional`, `Orientation`, `Refresher` |
| `departmentId` | guid | — | Departman filtresi |
| `instructorId` | guid | — | Eğitmen filtresi |
| `dateFrom` | date | — | Başlangıç tarihi |
| `dateTo` | date | — | Bitiş tarihi |
| `sortBy` | string | `createdAt` | Sıralama |
| `sortOrder` | string | `desc` | Yön |

**Response: `200 OK`**
```json
{
  "data": [
    {
      "id": "tr-001-...",
      "title": "Genel İSG Eğitimi",
      "type": "Mandatory",
      "category": {
        "name": "Temel İSG",
        "code": "ISG-001",
        "isLegal": true,
        "legalReference": "6331 Sayılı Kanun Md. 17"
      },
      "status": "Published",
      "duration": { "hours": 8, "minutes": 0 },
      "recurrencePeriod": { "months": 12 },
      "instructorName": "Dr. Ayşe Demir",
      "maxParticipants": 30,
      "sessionsCount": 3,
      "nextSessionDate": "2026-07-15T09:00:00Z",
      "createdAt": "2026-06-01T10:00:00Z"
    }
  ],
  "pagination": { ... }
}
```

---

### 5.2 GET `/v1/trainings/{id}`
🔒 **Requires:** `trainings.view`

**Response: `200 OK`**
```json
{
  "data": {
    "id": "tr-001-...",
    "title": "Genel İSG Eğitimi",
    "description": "6331 sayılı kanun kapsamında zorunlu İSG eğitimi",
    "type": "Mandatory",
    "category": {
      "name": "Temel İSG",
      "code": "ISG-001",
      "isLegal": true,
      "legalReference": "6331 Sayılı Kanun Md. 17"
    },
    "status": "Published",
    "duration": { "hours": 8, "minutes": 0 },
    "recurrencePeriod": { "months": 12 },
    "instructor": {
      "id": "usr-001-...",
      "fullName": "Dr. Ayşe Demir"
    },
    "maxParticipants": 30,
    "sessions": [
      {
        "id": "ses-001-...",
        "startDate": "2026-07-15T09:00:00Z",
        "endDate": "2026-07-15T17:00:00Z",
        "location": "Ana Bina - Toplantı Salonu A",
        "capacity": 30,
        "enrolledCount": 18,
        "status": "Scheduled"
      }
    ],
    "materials": [
      {
        "id": "mat-001-...",
        "title": "İSG Temel Kavramlar Sunumu",
        "type": "Presentation",
        "fileSize": 2048000,
        "downloadUrl": "/v1/trainings/tr-001/materials/mat-001/download"
      }
    ],
    "createdAt": "2026-06-01T10:00:00Z",
    "updatedAt": "2026-06-15T14:30:00Z"
  }
}
```

---

### 5.3 POST `/v1/trainings`
🔒 **Requires:** `trainings.create`

**Request:**
```json
{
  "title": "Yüksekte Çalışma Eğitimi",
  "description": "Yüksekte çalışma güvenliği ve kişisel koruyucu donanım kullanımı",
  "type": "Mandatory",
  "categoryCode": "ISG-005",
  "durationHours": 4,
  "durationMinutes": 0,
  "recurrencePeriodMonths": 12,
  "instructorId": "usr-001-...",
  "maxParticipants": 20
}
```

**Response: `201 Created`**
- `Location: /v1/trainings/new-training-id`

---

### 5.4 PUT `/v1/trainings/{id}`
🔒 **Requires:** `trainings.update`

### 5.5 PATCH `/v1/trainings/{id}/publish`
🔒 **Requires:** `trainings.update`

Eğitimi "Draft" → "Published" durumuna geçirir.

**Response: `200 OK`**

---

### 5.6 Training Sessions

#### POST `/v1/trainings/{trainingId}/sessions`
🔒 **Requires:** `trainings.update`

**Request:**
```json
{
  "startDate": "2026-08-01T09:00:00Z",
  "endDate": "2026-08-01T13:00:00Z",
  "location": "B Blok - Eğitim Salonu",
  "capacity": 25
}
```

#### DELETE `/v1/trainings/{trainingId}/sessions/{sessionId}`
🔒 **Requires:** `trainings.update`

#### PATCH `/v1/trainings/{trainingId}/sessions/{sessionId}/complete`
🔒 **Requires:** `trainings.complete`

**Request:**
```json
{
  "results": [
    {
      "employeeId": "emp-001-...",
      "attendanceStatus": "Attended",
      "score": 85,
      "isPassed": true
    },
    {
      "employeeId": "emp-002-...",
      "attendanceStatus": "Absent",
      "score": null,
      "isPassed": false
    }
  ]
}
```

---

### 5.7 Training Participants

#### POST `/v1/trainings/{trainingId}/sessions/{sessionId}/participants`
🔒 **Requires:** `trainings.manage-participants`

**Request:**
```json
{
  "employeeIds": ["emp-001-...", "emp-002-...", "emp-003-..."]
}
```

**Response: `200 OK`**
```json
{
  "data": {
    "enrolled": 3,
    "skipped": 0,
    "errors": []
  }
}
```

#### DELETE `/v1/trainings/{trainingId}/sessions/{sessionId}/participants/{employeeId}`
🔒 **Requires:** `trainings.manage-participants`

---

### 5.8 Training Materials

#### POST `/v1/trainings/{trainingId}/materials`
🔒 **Requires:** `trainings.update`  
**Content-Type:** `multipart/form-data`

| Field | Type | Validation |
|-------|------|-----------|
| `title` | string | Required, max 200 |
| `file` | file | Max 50MB, allowed: pdf, pptx, docx, mp4, jpg, png |

#### GET `/v1/trainings/{trainingId}/materials/{materialId}/download`
🔒 **Requires:** `trainings.view`

---

## 6. Certificate API

### 6.1 GET `/v1/certificates`
🔒 **Requires:** `certificates.view`

**Query Parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `status` | string | `Active`, `ExpiringSoon`, `Expired`, `Revoked` |
| `employeeId` | guid | Çalışan filtresi |
| `trainingId` | guid | Eğitim filtresi |
| `expiringWithinDays` | int | Belirtilen gün içinde süresi dolacaklar |

**Response: `200 OK`**
```json
{
  "data": [
    {
      "id": "cert-001-...",
      "certificateNumber": "SF-2026-00042",
      "employeeName": "Ahmet Yılmaz",
      "trainingTitle": "Genel İSG Eğitimi",
      "status": "Active",
      "issuedAt": "2026-07-01T10:00:00Z",
      "expiresAt": "2027-07-01T10:00:00Z",
      "daysUntilExpiry": 359
    }
  ]
}
```

---

### 6.2 GET `/v1/certificates/{id}`
🔒 **Requires:** `certificates.view`

### 6.3 GET `/v1/certificates/{id}/download`
🔒 **Requires:** `certificates.download`

PDF formatında sertifika indirir.

**Response: `200 OK`**
- `Content-Type: application/pdf`
- `Content-Disposition: attachment; filename="SF-2026-00042.pdf"`

### 6.4 PATCH `/v1/certificates/{id}/revoke`
🔒 **Requires:** `certificates.revoke`

**Request:**
```json
{
  "reason": "Eğitim içeriği güncellendiği için eski sertifikalar geçersiz kılınmıştır."
}
```

### 6.5 GET `/v1/certificates/expiring`
🔒 **Requires:** `certificates.view`

Yaklaşan sertifika bitiş tarihlerinin özet raporu.

**Response: `200 OK`**
```json
{
  "data": {
    "expiring7Days": 5,
    "expiring30Days": 12,
    "expiring90Days": 34,
    "items": [
      {
        "certificateId": "cert-001-...",
        "certificateNumber": "SF-2026-00042",
        "employeeName": "Ahmet Yılmaz",
        "trainingTitle": "Genel İSG Eğitimi",
        "expiresAt": "2026-07-15T10:00:00Z",
        "daysRemaining": 8
      }
    ]
  }
}
```

---

## 7. Company & Employee API

### 7.1 GET `/v1/company`
🔒 **Requires:** `companies.view`

Mevcut tenant'ın şirket bilgilerini döner.

### 7.2 PUT `/v1/company`
🔒 **Requires:** `companies.update`

### 7.3 Departments

#### GET `/v1/departments`
🔒 **Requires:** `companies.view`

**Response: `200 OK`**
```json
{
  "data": [
    {
      "id": "dept-001-...",
      "name": "Üretim",
      "managerName": "Ali Veli",
      "parentDepartmentId": null,
      "employeeCount": 45,
      "isActive": true,
      "subDepartments": [
        {
          "id": "dept-002-...",
          "name": "Montaj",
          "employeeCount": 20
        }
      ]
    }
  ]
}
```

#### POST `/v1/departments`
🔒 **Requires:** `companies.manage-departments`

#### PUT `/v1/departments/{id}`
#### DELETE `/v1/departments/{id}`

---

### 7.4 Employees

#### GET `/v1/employees`
🔒 **Requires:** Authenticated

**Query Parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `search` | string | İsim, TC, pozisyon araması |
| `departmentId` | guid | Departman filtresi |
| `locationId` | guid | Lokasyon filtresi |
| `status` | string | `Active`, `OnLeave`, `Terminated` |

#### GET `/v1/employees/{id}`

**Response: `200 OK`**
```json
{
  "data": {
    "id": "emp-001-...",
    "fullName": "Ahmet Yılmaz",
    "tcKimlikNo": "123456789**",
    "department": { "id": "dept-001-...", "name": "Üretim" },
    "location": { "id": "loc-001-...", "name": "Ana Fabrika" },
    "employment": {
      "position": "Üretim Operatörü",
      "title": "Kıdemli Operatör",
      "startDate": "2020-03-15",
      "type": "FullTime",
      "sgkNo": "123456789"
    },
    "emergencyContact": {
      "name": "Fatma Yılmaz",
      "relationship": "Eş",
      "phone": "+905551112233"
    },
    "status": "Active",
    "trainingSummary": {
      "completedCount": 5,
      "pendingCount": 2,
      "expiringSoonCount": 1
    },
    "certificateSummary": {
      "activeCount": 4,
      "expiringSoonCount": 1,
      "expiredCount": 0
    }
  }
}
```

#### POST `/v1/employees`
#### PUT `/v1/employees/{id}`
#### PATCH `/v1/employees/{id}/transfer`

**Request:**
```json
{
  "departmentId": "dept-002-...",
  "locationId": "loc-002-...",
  "effectiveDate": "2026-08-01"
}
```

---

## 8. Dashboard API

### 8.1 GET `/v1/dashboard/summary`
🔒 **Requires:** `dashboard.view`

**Response: `200 OK`**
```json
{
  "data": {
    "overview": {
      "totalEmployees": 450,
      "activeTrainings": 12,
      "completedTrainingsThisMonth": 8,
      "certificatesExpiringSoon": 15,
      "complianceRate": 87.5
    },
    "trainingStats": {
      "thisMonth": { "planned": 5, "completed": 3, "cancelled": 0 },
      "participationRate": 92.3,
      "averageScore": 78.5
    },
    "certificateStats": {
      "active": 380,
      "expiringSoon": 15,
      "expired": 8,
      "revoked": 2
    },
    "upcomingTrainings": [
      {
        "id": "tr-001-...",
        "title": "Yangın Güvenliği Eğitimi",
        "date": "2026-07-15T09:00:00Z",
        "enrolledCount": 22,
        "capacity": 30
      }
    ],
    "recentActivities": [
      {
        "type": "TrainingCompleted",
        "description": "Genel İSG Eğitimi tamamlandı",
        "timestamp": "2026-07-06T16:30:00Z",
        "actorName": "Dr. Ayşe Demir"
      }
    ]
  }
}
```

### 8.2 GET `/v1/dashboard/trends`
🔒 **Requires:** `dashboard.view`

**Query Parameters:**
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| `period` | string | `6months` | `1month`, `3months`, `6months`, `1year` |
| `departmentId` | guid | — | Departman bazlı (opsiyonel) |

**Response: `200 OK`**
```json
{
  "data": {
    "trainingTrend": [
      { "month": "2026-02", "completed": 5, "planned": 7, "participationRate": 88.2 },
      { "month": "2026-03", "completed": 8, "planned": 8, "participationRate": 95.1 },
      { "month": "2026-04", "completed": 6, "planned": 9, "participationRate": 82.7 }
    ],
    "complianceTrend": [
      { "month": "2026-02", "rate": 82.1 },
      { "month": "2026-03", "rate": 85.3 },
      { "month": "2026-04", "rate": 87.5 }
    ],
    "departmentComparison": [
      { "department": "Üretim", "complianceRate": 91.2, "avgScore": 82.1, "employeeCount": 120 },
      { "department": "Depo", "complianceRate": 78.5, "avgScore": 71.3, "employeeCount": 45 },
      { "department": "Ofis", "complianceRate": 95.0, "avgScore": 88.7, "employeeCount": 30 }
    ]
  }
}
```

### 8.3 GET `/v1/dashboard/ai-insights` (Phase 3)
🔒 **Requires:** `dashboard.view`

**Response: `200 OK`**
```json
{
  "data": {
    "insights": [
      {
        "type": "RiskWarning",
        "severity": "High",
        "title": "Depo departmanında eğitim uyum oranı düşüyor",
        "description": "Son 3 ayda Depo departmanının eğitim katılım oranı %78.5'e düştü. Zorunlu eğitimlerin planlanması önerilir.",
        "suggestedAction": "Depo departmanı için Genel İSG Eğitimi planla",
        "confidence": 0.87,
        "generatedAt": "2026-07-07T06:00:00Z"
      }
    ]
  }
}
```

---

## 9. Report API

### 9.1 POST `/v1/reports/generate`
🔒 **Requires:** `reports.generate`

**Request:**
```json
{
  "reportType": "TrainingComplianceReport",
  "format": "PDF",
  "parameters": {
    "departmentId": "dept-001-...",
    "dateFrom": "2026-01-01",
    "dateTo": "2026-06-30"
  }
}
```

**Response: `202 Accepted`**
```json
{
  "data": {
    "reportId": "rpt-001-...",
    "status": "Processing",
    "estimatedCompletionSeconds": 15
  }
}
```

### 9.2 GET `/v1/reports/{id}/status`
🔒 **Requires:** `reports.view`

### 9.3 GET `/v1/reports/{id}/download`
🔒 **Requires:** `reports.view`

### 9.4 Desteklenen Rapor Tipleri

| Report Type | Formatlar | Açıklama |
|-------------|-----------|----------|
| `TrainingComplianceReport` | PDF, Excel, CSV | Eğitim uyum raporu |
| `CertificateStatusReport` | PDF, Excel | Sertifika durum raporu |
| `EmployeeTrainingHistory` | PDF, Excel | Çalışan eğitim geçmişi |
| `DepartmentComplianceReport` | PDF, Excel | Departman bazlı uyum |
| `ExpiringCertificatesReport` | PDF, Excel, CSV | Süresi dolacak sertifikalar |
| `TrainingAttendanceReport` | PDF, Excel | Eğitim katılım raporu |

---

## 10. Sync API (Mobile Offline Support)

### 10.1 POST `/v1/sync/pull`
🔒 **Requires:** Authenticated

Mobil cihazdan son senkronizasyondan bu yana değişen verileri çeker.

**Request:**
```json
{
  "lastSyncTimestamp": "2026-07-06T10:00:00Z",
  "entities": ["trainings", "certificates", "employees", "notifications"]
}
```

**Response: `200 OK`**
```json
{
  "data": {
    "syncTimestamp": "2026-07-07T10:00:00Z",
    "changes": {
      "trainings": {
        "created": [ { ... } ],
        "updated": [ { ... } ],
        "deleted": ["tr-old-001-..."]
      },
      "certificates": {
        "created": [ { ... } ],
        "updated": [ { ... } ],
        "deleted": []
      }
    },
    "hasMore": false
  }
}
```

### 10.2 POST `/v1/sync/push`
🔒 **Requires:** Authenticated

Mobil cihazdan offline yapılan değişiklikleri sunucuya gönderir.

**Request:**
```json
{
  "clientId": "device-001-...",
  "operations": [
    {
      "id": "op-001",
      "entityType": "TrainingParticipation",
      "entityId": "tp-001-...",
      "operationType": "Update",
      "data": { "attendanceStatus": "Attended", "score": 85 },
      "timestamp": "2026-07-06T14:30:00Z"
    }
  ]
}
```

**Response: `200 OK`**
```json
{
  "data": {
    "processed": 1,
    "conflicts": [],
    "errors": []
  }
}
```

### 10.3 GET `/v1/sync/status`
🔒 **Requires:** Authenticated

---

## 11. Notification API

### 11.1 GET `/v1/notifications`
🔒 **Requires:** Authenticated

**Query Parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `isRead` | bool | Okunmuş/okunmamış filtresi |
| `type` | string | Bildirim tipi filtresi |

### 11.2 PATCH `/v1/notifications/{id}/read`
### 11.3 PATCH `/v1/notifications/read-all`
### 11.4 GET `/v1/notifications/unread-count`

**Response: `200 OK`**
```json
{
  "data": { "count": 7 }
}
```

---

## 12. Health Check API

### 12.1 GET `/health`
🔒 **Public** — Genel sağlık durumu

**Response: `200 OK`**
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0234567",
  "entries": {
    "database": { "status": "Healthy", "duration": "00:00:00.0123456" },
    "cache": { "status": "Healthy", "duration": "00:00:00.0012345" }
  }
}
```

### 12.2 GET `/health/live`
🔒 **Public** — Liveness probe (Kubernetes). Uygulama çalışıyor mu?

**Response: `200 OK`** veya `503 Service Unavailable`

### 12.3 GET `/health/ready`
🔒 **Public** — Readiness probe (Kubernetes). Uygulama istek almaya hazır mı?

Kontrol eder: DB bağlantısı, cache bağlantısı, disk alanı.

**Response: `200 OK`** veya `503 Service Unavailable`

---

## 13. Role & Permission API

### 13.1 GET `/v1/roles`
🔒 **Requires:** `system.manage-settings`

### 13.2 POST `/v1/roles`
### 13.3 PUT `/v1/roles/{id}`
### 13.4 GET `/v1/roles/{id}/permissions`
### 13.5 PUT `/v1/roles/{id}/permissions`

**Request:**
```json
{
  "permissions": [
    "trainings.view",
    "trainings.create",
    "certificates.view",
    "dashboard.view"
  ]
}
```

---

## 14. Varsayılan Roller

| Rol | Açıklama | Temel İzinler |
|-----|----------|---------------|
| **SystemAdmin** | Sistem yöneticisi | Tüm izinler |
| **CompanyAdmin** | Şirket yöneticisi | Şirket + kullanıcı + rapor |
| **ISGUzmani** | İSG uzmanı | Eğitim + sertifika + risk + denetim |
| **ISGHekim** | İşyeri hekimi | Sağlık gözetimi + eğitim (görüntüleme) |
| **DepartmentManager** | Departman müdürü | Kendi departmanı + dashboard |
| **Employee** | Çalışan | Kendi bilgileri + eğitim katılım |

---

## 15. Rate Limiting Politikası

| Endpoint Grubu | Limit | Pencere |
|----------------|-------|---------|
| `POST /v1/auth/login` | 5 istek | 15 dakika |
| `POST /v1/auth/forgot-password` | 3 istek | 1 saat |
| `GET /v1/*` (authenticated) | 100 istek | 1 dakika |
| `POST/PUT/PATCH /v1/*` | 30 istek | 1 dakika |
| `POST /v1/reports/generate` | 5 istek | 10 dakika |
| `POST /v1/sync/*` | 10 istek | 1 dakika |

---

## 16. Request/Response Header'ları

### Request Header'ları

| Header | Zorunlu | Açıklama |
|--------|---------|----------|
| `Authorization` | ✅ (korumalı) | `Bearer {accessToken}` |
| `Content-Type` | ✅ (body varsa) | `application/json` veya `multipart/form-data` |
| `Accept-Language` | ❌ | `tr-TR`, `en-US` (varsayılan: `tr-TR`) |
| `X-Idempotency-Key` | ❌ | POST istekleri için idempotency |
| `X-Client-Version` | ❌ | Mobil uygulama versiyonu |
| `X-Device-Id` | ❌ | Mobil cihaz tanımlayıcı |

### Response Header'ları

| Header | Açıklama |
|--------|----------|
| `X-Correlation-Id` | İstek izleme kimliği |
| `X-RateLimit-Limit` | Rate limit üst sınırı |
| `X-RateLimit-Remaining` | Kalan istek hakkı |
| `X-RateLimit-Reset` | Reset zamanı (Unix timestamp) |
| `X-Request-Duration` | İstek süresi (ms) |

---

## 17. JWT Token Yapısı

### Access Token Claims

```json
{
  "sub": "a1b2c3d4-...",
  "email": "ahmet.yilmaz@company.com",
  "name": "Ahmet Yılmaz",
  "tenant_id": "t1t2t3t4-...",
  "roles": ["ISGUzmani"],
  "permissions": [
    "trainings.view",
    "trainings.create",
    "trainings.update",
    "certificates.view",
    "dashboard.view"
  ],
  "iat": 1720339200,
  "exp": 1720340100,
  "iss": "safeflow-api",
  "aud": "safeflow-client"
}
```

---

## 18. Dosya Yükleme Güvenlik Kuralları

| Kural | Değer |
|-------|-------|
| Maksimum dosya boyutu | 50 MB |
| İzin verilen MIME tipleri | `application/pdf`, `application/vnd.openxmlformats-officedocument.*`, `image/jpeg`, `image/png`, `video/mp4` |
| Dosya adı sanitizasyonu | Özel karakterler kaldırılır, UUID prefix eklenir |
| Virus taraması | Production ortamda aktif (ClamAV) |
| Depolama | Doğrudan erişim yok, signed URL ile indirme |

---

## Açık Sorular

| # | Soru | Etki |
|---|------|------|
| 1 | API versiyonlama stratejisi: sadece URL-based mi, yoksa header-based de desteklenecek mi? | Tüm endpoint'ler |
| 2 | Pagination: Offset-based mi yoksa cursor-based mi tercih edilecek? | Liste endpoint'leri |
| 3 | Webhook desteği eklenmeli mi? (Örn: sertifika oluşturulunca harici sisteme bildirim) | Entegrasyon |
| 4 | GraphQL desteği gelecekte planlanıyor mu? | API Gateway |
| 5 | Dosya yükleme için pre-signed URL yaklaşımı mı, yoksa doğrudan API üzerinden mi? | Dosya endpoint'leri |
