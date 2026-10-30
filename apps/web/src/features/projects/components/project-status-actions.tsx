import { ConfirmAction } from '@/shared/components/confirm-action'
import { PROJECT_STATUS_LABELS, projectApi, type Project, type ProjectStatus } from '../api/project-api'

const TRANSITIONS: Record<ProjectStatus, { to: ProjectStatus; label: string }[]> = {
  InProgress: [{ to: 'Completed', label: 'Hoàn thành' }],
  Completed: [
    { to: 'InProgress', label: 'Mở lại' },
    { to: 'Closed', label: 'Đóng' }
  ],
  Closed: [{ to: 'Completed', label: 'Mở lại' }]
}

interface ProjectStatusActionsProps {
  project: Project
  onChanged: () => Promise<unknown>
}

/** Các nút đổi trạng thái hợp lệ của dự án: Đang thực hiện ↔ Hoàn thành ↔ Đóng. */
export function ProjectStatusActions({ project, onChanged }: ProjectStatusActionsProps) {
  return TRANSITIONS[project.status].map(({ to, label }) => (
    <ConfirmAction
      key={to}
      label={label}
      title={`Chuyển dự án ${project.code} sang "${PROJECT_STATUS_LABELS[to]}"?`}
      content={
        to === 'InProgress' ? undefined : 'Dự án sẽ không chọn được khi lập chứng từ mới; chứng từ đã có giữ nguyên.'
      }
      onConfirm={async () => {
        await projectApi.changeStatus(project.id, to, project.version)
        await onChanged()
      }}
    />
  ))
}
