import assert from 'node:assert/strict'
import test from 'node:test'
import { resolveRef } from './resolveRef.mjs'

test('removes unresolved object properties while resolving references', () => {
  const refs = new Map([
    [
      '#/components/schemas/Root',
      {
        type: 'object',
        properties: {
          valid: { $ref: '#/components/schemas/Valid' },
          missing: { $ref: '#/components/schemas/Missing' },
        },
      },
    ],
    ['#/components/schemas/Valid', { type: 'string' }],
    ['#/components/schemas/Missing', undefined],
  ])

  const resolved = resolveRef('#/components/schemas/Root', refs)

  assert.deepEqual(resolved.properties, {
    valid: { type: 'string' },
  })
})

test('preserves a reference when resolving a recursive schema', () => {
  const refs = new Map([
    [
      '#/components/schemas/Node',
      {
        type: 'object',
        properties: {
          value: { type: 'string' },
          next: { $ref: '#/components/schemas/Node' },
        },
      },
    ],
  ])

  const resolved = resolveRef('#/components/schemas/Node', refs)

  assert.deepEqual(resolved.properties, {
    value: { type: 'string' },
    next: { $ref: '#/components/schemas/Node' },
  })
})
