import clsx from 'clsx'

type Tone = 'success' | 'warning' | 'neutral' | 'danger'

interface Props {
  label: string
  tone?: Tone
}

const toneMap: Record<Tone, string> = {
  success: 'bg-green-100 text-green-800',
  warning: 'bg-yellow-100 text-yellow-800',
  neutral: 'bg-slate-100 text-slate-800',
  danger: 'bg-red-100 text-red-800',
}

export function Badge({ label, tone = 'neutral' }: Props) {
  return (
    <span className={clsx('inline-flex items-center rounded-full px-2.5 py-1 text-xs font-medium', toneMap[tone])}>
      {label}
    </span>
  )
}
