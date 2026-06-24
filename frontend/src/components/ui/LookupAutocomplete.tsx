import { Autocomplete, TextField, type FilterOptionsState } from '@mui/material'
import { useCallback, useEffect, useRef, useState } from 'react'
import type { LookupItem } from '../../types'
import { getApiErrorMessage } from '../../utils/apiError'
import { formatLookupLabel } from '../../utils/formatters'

const LISTBOX_MAX_OPTIONS = 100

interface LookupAutocompleteBaseProps {
  label: string
  placeholder?: string
  loadOptions: () => Promise<LookupItem[]>
  disabled?: boolean
  required?: boolean
  helperText?: string
  error?: boolean
  /** When true, fetches options on first dropdown open instead of on mount. */
  loadOnOpen?: boolean
}

interface SingleLookupAutocompleteProps extends LookupAutocompleteBaseProps {
  multiple?: false
  value: LookupItem | null
  onChange: (value: LookupItem | null) => void
}

interface MultipleLookupAutocompleteProps extends LookupAutocompleteBaseProps {
  multiple: true
  value: LookupItem[]
  onChange: (value: LookupItem[]) => void
}

export type LookupAutocompleteProps = SingleLookupAutocompleteProps | MultipleLookupAutocompleteProps

export function LookupAutocomplete(props: LookupAutocompleteProps) {
  const {
    label,
    placeholder,
    loadOptions,
    disabled,
    required,
    helperText,
    error,
    multiple = false,
    loadOnOpen = false,
  } = props

  const [options, setOptions] = useState<LookupItem[]>([])
  const [loading, setLoading] = useState(!loadOnOpen)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [loaded, setLoaded] = useState(false)
  const loadOptionsRef = useRef(loadOptions)
  loadOptionsRef.current = loadOptions

  const fetchOptions = useCallback(async () => {
    setLoading(true)
    setLoadError(null)
    try {
      const items = await loadOptionsRef.current()
      setOptions(items)
      setLoaded(true)
      if (items.length === 0) {
        setLoadError('Nenhuma opção disponível. Verifique se a ingestão foi concluída.')
      }
    } catch (fetchError) {
      setOptions([])
      setLoadError(getApiErrorMessage(fetchError))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    if (!loadOnOpen) {
      void fetchOptions()
    }
  }, [fetchOptions, loadOnOpen])

  const handleOpen = useCallback(() => {
    if (!loaded && !loading) {
      void fetchOptions()
    }
  }, [fetchOptions, loaded, loading])

  const filterOptions = useCallback((opts: LookupItem[], state: FilterOptionsState<LookupItem>) => {
    const query = state.inputValue.trim().toLowerCase()
    const filtered = query
      ? opts.filter(
          (option) =>
            option.code.toLowerCase().includes(query) ||
            option.description.toLowerCase().includes(query),
        )
      : opts

    return filtered.slice(0, LISTBOX_MAX_OPTIONS)
  }, [])

  const noOptionsText = loading
    ? 'Carregando…'
    : loadError
      ? loadError
      : options.length > LISTBOX_MAX_OPTIONS
        ? 'Digite para refinar a busca'
        : 'Nenhum resultado'

  const commonProps = {
    options,
    loading,
    disabled,
    onOpen: handleOpen,
    getOptionLabel: formatLookupLabel,
    isOptionEqualToValue: (a: LookupItem, b: LookupItem) => a.code === b.code,
    filterOptions,
    noOptionsText,
    slotProps: {
      listbox: {
        sx: { maxHeight: 280 },
      },
      popper: {
        sx: { zIndex: (theme: { zIndex: { modal: number } }) => theme.zIndex.modal + 1 },
      },
    },
    renderInput: (params: object) => (
      <TextField
        {...params}
        label={label}
        placeholder={placeholder}
        required={required}
        helperText={loadError ?? helperText}
        error={error || loadError !== null}
      />
    ),
  }

  if (multiple) {
    const { value, onChange } = props as MultipleLookupAutocompleteProps
    return (
      <Autocomplete
        {...commonProps}
        multiple
        disableCloseOnSelect
        value={value}
        onChange={(_event, newValue) => onChange(newValue)}
      />
    )
  }

  const { value, onChange } = props as SingleLookupAutocompleteProps
  return (
    <Autocomplete
      {...commonProps}
      value={value}
      onChange={(_event, newValue) => onChange(newValue)}
    />
  )
}
