import {
  Autocomplete,
  TextField,
  type AutocompleteProps,
  type FilterOptionsState,
} from '@mui/material'
import { useCallback, useEffect, useRef, useState, type UIEvent } from 'react'
import type { PagedResult } from '../../types/common'

const MIN_SEARCH_LENGTH = 3
const DEBOUNCE_MS = 300
const SCROLL_LOAD_THRESHOLD_PX = 8

export interface PaginatedAutocompleteProps<T> {
  label: string
  placeholder?: string
  value: T | null
  onChange: (value: T | null) => void
  fetchPage: (query: string, page: number) => Promise<PagedResult<T>>
  getOptionLabel: (option: T) => string
  isOptionEqualToValue?: (option: T, value: T) => boolean
  renderOption?: AutocompleteProps<T, false, false, false>['renderOption']
  disabled?: boolean
  required?: boolean
  helperText?: string
  error?: boolean
  noOptionsText?: string
}

export function PaginatedAutocomplete<T>({
  label,
  placeholder,
  value,
  onChange,
  fetchPage,
  getOptionLabel,
  isOptionEqualToValue,
  renderOption,
  disabled,
  required,
  helperText,
  error,
  noOptionsText = 'Digite pelo menos 3 caracteres para buscar',
}: PaginatedAutocompleteProps<T>) {
  const [inputValue, setInputValue] = useState('')
  const [options, setOptions] = useState<T[]>([])
  const [loading, setLoading] = useState(false)
  const [page, setPage] = useState(1)
  const [total, setTotal] = useState(0)
  const [searchQuery, setSearchQuery] = useState('')
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const hasMore = options.length < total

  const loadPage = useCallback(
    async (query: string, pageNumber: number, append: boolean) => {
      if (query.length < MIN_SEARCH_LENGTH) {
        setOptions([])
        setTotal(0)
        setPage(1)
        return
      }

      setLoading(true)
      try {
        const result = await fetchPage(query, pageNumber)
        setTotal(result.total)
        setPage(pageNumber)
        setOptions((prev) => (append ? [...prev, ...result.items] : result.items))
      } finally {
        setLoading(false)
      }
    },
    [fetchPage],
  )

  useEffect(() => {
    if (debounceRef.current) {
      clearTimeout(debounceRef.current)
    }

    debounceRef.current = setTimeout(() => {
      setSearchQuery(inputValue.trim())
      void loadPage(inputValue.trim(), 1, false)
    }, DEBOUNCE_MS)

    return () => {
      if (debounceRef.current) {
        clearTimeout(debounceRef.current)
      }
    }
  }, [inputValue, loadPage])

  useEffect(() => {
    if (value) {
      setInputValue(getOptionLabel(value))
    }
  }, [value, getOptionLabel])

  const filterOptions = useCallback((opts: T[], _state: FilterOptionsState<T>) => opts, [])

  const handleListboxScroll = useCallback(
    (event: UIEvent<HTMLUListElement>) => {
      if (!hasMore || loading || searchQuery.length < MIN_SEARCH_LENGTH) {
        return
      }

      const node = event.currentTarget
      const nearBottom =
        node.scrollTop + node.clientHeight >= node.scrollHeight - SCROLL_LOAD_THRESHOLD_PX

      if (nearBottom) {
        void loadPage(searchQuery, page + 1, true)
      }
    },
    [hasMore, loading, loadPage, page, searchQuery],
  )

  return (
    <Autocomplete
      value={value}
      onChange={(_event, newValue) => onChange(newValue)}
      inputValue={inputValue}
      onInputChange={(_event, newInputValue, reason) => {
        if (reason === 'clear') {
          onChange(null)
        }
        setInputValue(newInputValue)
      }}
      options={options}
      loading={loading}
      disabled={disabled}
      filterOptions={filterOptions}
      getOptionLabel={getOptionLabel}
      isOptionEqualToValue={isOptionEqualToValue}
      renderOption={renderOption}
      noOptionsText={
        inputValue.trim().length > 0 && inputValue.trim().length < MIN_SEARCH_LENGTH
          ? noOptionsText
          : loading
            ? 'Buscando…'
            : 'Nenhum resultado'
      }
      slotProps={{
        listbox: {
          onScroll: handleListboxScroll,
          sx: { maxHeight: 280 },
        },
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
          helperText={helperText}
          error={error}
        />
      )}
    />
  )
}
