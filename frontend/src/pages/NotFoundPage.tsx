import { Button, Stack } from '@mui/material'
import { Link } from 'react-router'
import { PageHeader } from '../components/ui'

export function NotFoundPage() {
  return (
    <Stack spacing={3}>
      <PageHeader
        title="Página não encontrada"
        description="O endereço acessado não existe ou foi movido."
      />
      <Button component={Link} to="/" variant="contained">
        Voltar ao início
      </Button>
    </Stack>
  )
}
