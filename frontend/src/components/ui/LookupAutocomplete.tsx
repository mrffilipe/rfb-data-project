import { Autocomplete, TextField } from '@mui/material'
import { useEffect, useState } from 'react'
import type { LookupItem } from '../../types'
import { formatLookupLabel } from '../../utils/formatters'

export interface LookupAutocompleteProps {
  label: string
  placeholder?: string
  value: LookupItem | null
  onChange: (value: LookupItem | null) => void
  loadOptions: () => Promise<LookupItem[]>
  disabled?: boolean
  required?: boolean
  helperText?: string
  error?: boolean
}

export function LookupAutocomplete({
  label,
  placeholder,
  value,
  onChange,
  loadOptions,
  disabled,
  required,
  helperText,
  error,
}: LookupAutocompleteProps) {
  const [options, setOptions] = useState<LookupItem[]>([])
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false

    async function load(): Promise<void> {
      setLoading(true)
      setLoadError(null)
      try {
        const items = await loadOptions()
        if (!cancelled) {
          setOptions(items)
        }
      } catch {
        if (!cancelled) {
          setLoadError('Não foi possível carregar as opções.')
          setOptions([])
        }
      } finally {
        if (!cancelled) {
          setLoading(false)
        }
      }
    }

    void load()

    return () => {
      cancelled = true
    }
  }, [loadOptions])

  return (
    <Autocomplete
      value={value}
      onChange={(_event, newValue) => onChange(newValue)}
      options={options}
      loading={loading}
      disabled={disabled || loading}
      getOptionLabel={formatLookupLabel}
      isOptionEqualToValue={(a, b) => a.code === b.code}
      filterOptions={(opts, state) => {
        const query = state.inputValue.trim().toLowerCase()
        if (!query) {
          return opts
        }
        return opts.filter(
          (option) =>
            option.code.toLowerCase().includes(query) ||
            option.description.toLowerCase().includes(query),
        )
      }}
      noOptionsText={loading ? 'Carregando…' : 'Nenhum resultado'}
      slotProps={{
        popper: {
          sx: { zIndex: (theme) => theme.zIndex.modal + 1 },
        },
      }}
      renderInput={(params) => (
        <TextField
          {...params}
          label={label}
          placeholder={placeholder}
          required={required}
          helperText={loadError ?? helperText}
          error={error || loadError !== null}
        />
      )}
    />
  )
}
