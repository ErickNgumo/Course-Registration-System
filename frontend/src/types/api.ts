// ─── Auth ────────────────────────────────────────────────

export interface StudentLoginRequest {
  email: string;
  password: string;
}

export interface StudentLoginResponse {
  accessToken: string;
  tokenType: string;
  expiresIn: number;
  student: StudentDto;
}

export interface StudentDto {
  id: string;
  studentNumber: string;
  firstName: string;
  lastName: string;
  email: string;
}

export interface AdministratorLoginRequest {
  email: string;
  password: string;
}

export interface AdministratorLoginResponse {
  accessToken: string;
  tokenType: string;
  expiresIn: number;
  administrator: AdministratorDto;
}

export interface AdministratorDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
}

// ─── JWT Claims ──────────────────────────────────────────

export interface JwtClaims {
  sub: string;
  nameid: string;
  email: string;
  role: "Student" | "Administrator";
  student_number?: string;
  exp: number;
  iss: string;
  aud: string;
}

// ─── Courses ─────────────────────────────────────────────

export interface CourseResponse {
  id: string;
  code: string;
  name: string;
  description: string | null;
  credits: number;
  capacity: number;
  semester: string;
}

// ─── Enrollments ─────────────────────────────────────────

export type EnrollmentStatus =
  | "Registered"
  | "Waitlisted"
  | "Dropped"
  | "Completed";

export interface RegisterEnrollmentRequest {
  courseId: string;
}

export interface EnrollmentResponse {
  id: string;
  courseId: string;
  courseCode: string;
  courseName: string;
  semester: string;
  credits: number;
  status: EnrollmentStatus;
  registeredAt: string;
  droppedAt: string | null;
  finalGrade: string | null;
}

// ─── Student Dashboard ───────────────────────────────────

export interface DashboardResponse {
  studentId: string;
  studentNumber: string;
  firstName: string;
  lastName: string;
  email: string;
  currentSemesterCredits: number;
  maxSemesterCredits: number;
  registeredCourses: EnrollmentResponse[];
  waitlistedCourses: EnrollmentResponse[];
  completedCourses: EnrollmentResponse[];
}

// ─── Pagination ──────────────────────────────────────────

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

// ─── Admin Dashboard ─────────────────────────────────────

export interface AdminDashboardResponse {
  totalStudents: number;
  activeStudents: number;
  suspendedStudents: number;
  totalCourses: number;
  activeCourses: number;
  totalRegistrations: number;
  registeredStudents: number;
  waitlistedStudents: number;
  availableSeats: number;
  semesterStatistics: SemesterStatisticResponse[];
}

export interface SemesterStatisticResponse {
  semester: string;
  registered: number;
  waitlisted: number;
  completed: number;
  dropped: number;
}

// ─── Admin Students ──────────────────────────────────────

export type StudentStatus = "Active" | "Suspended" | "Inactive";

export interface StudentAdministrationResponse {
  id: string;
  studentNumber: string;
  firstName: string;
  lastName: string;
  email: string;
  status: StudentStatus;
}

export interface ChangeStudentStatusRequest {
  status: string;
}

// ─── Admin Courses ───────────────────────────────────────

export interface CourseAdministrationResponse {
  id: string;
  code: string;
  name: string;
  description: string | null;
  credits: number;
  capacity: number;
  semester: string;
  isActive: boolean;
  activeEnrollmentCount: number;
  schedules: ScheduleResponse[];
  prerequisiteCourseIds: string[];
}

export interface ScheduleResponse {
  id: string;
  dayOfWeek: number;
  startTime: string;
  endTime: string;
}

export interface ScheduleInput {
  dayOfWeek: number;
  startTime: string;
  endTime: string;
}

export interface SaveCourseRequest {
  code: string;
  name: string;
  description: string | null;
  credits: number;
  capacity: number;
  semester: string;
  schedules: ScheduleInput[];
  prerequisiteCourseIds: string[];
}

// ─── Admin Enrollments ───────────────────────────────────

export interface EnrollmentAdministrationResponse {
  id: string;
  studentId: string;
  studentEmail: string;
  courseId: string;
  courseCode: string;
  courseName: string;
  semester: string;
  status: string;
  registeredAt: string;
  droppedAt: string | null;
  finalGrade: string | null;
}

export interface AssignGradeRequest {
  finalGrade: string | null;
}

// ─── Admin Reports ───────────────────────────────────────

export interface CourseEnrollmentReport {
  courses: CourseEnrollmentDetailResponse[];
}

export interface CourseEnrollmentDetailResponse {
  courseId: string;
  code: string;
  name: string;
  credits: number;
  capacity: number;
  registered: number;
  waitlisted: number;
  availableSeats: number;
  utilizationPercent: number;
  semester: string;
  isActive: boolean;
}

export interface StudentsByStatusReport {
  statuses: StatusCountResponse[];
}

export interface StatusCountResponse {
  status: string;
  count: number;
}

export interface WaitlistReport {
  waitlists: WaitlistDetailResponse[];
}

export interface WaitlistDetailResponse {
  courseId: string;
  code: string;
  name: string;
  semester: string;
  waitlisted: number;
}

export interface AvailableSeatsReport {
  courses: CourseEnrollmentDetailResponse[];
}

// ─── Admin Audit ─────────────────────────────────────────

export interface AuditLogResponse {
  id: string;
  administratorId: string;
  action: string;
  entity: string;
  entityId: string;
  timestamp: string;
  oldValues: string | null;
  newValues: string | null;
}

// ─── Student Profile ─────────────────────────────────────

export interface StudentProfileResponse {
  student: StudentAdministrationResponse;
  currentRegistrations: EnrollmentAdministrationResponse[];
  completedCourses: EnrollmentAdministrationResponse[];
  waitlists: EnrollmentAdministrationResponse[];
  history: EnrollmentAdministrationResponse[];
}