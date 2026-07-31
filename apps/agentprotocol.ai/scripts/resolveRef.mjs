export function resolveRef(ref, refs, resolving = new Set()) {
  if (resolving.has(ref)) {
    return { $ref: ref }
  }

  const temp = refs.get(ref)
  if (!temp || !temp.properties) {
    return temp
  }

  resolving.add(ref)
  try {
    Object.keys(temp.properties).forEach((key) => {
      if (temp.properties[key]['$ref']) {
        temp.properties[key] = resolveRef(
          temp.properties[key]['$ref'],
          refs,
          resolving
        )
        if (!temp.properties[key]) {
          delete temp.properties[key]
        }
      }
    })
  } finally {
    resolving.delete(ref)
  }

  return temp
}
