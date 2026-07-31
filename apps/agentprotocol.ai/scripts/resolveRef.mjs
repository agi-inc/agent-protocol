export function resolveRef(ref, refs) {
  const temp = refs.get(ref)
  if (!temp || !temp.properties) {
    return temp
  }

  Object.keys(temp.properties).forEach((key) => {
    if (temp.properties[key]['$ref']) {
      temp.properties[key] = resolveRef(temp.properties[key]['$ref'], refs)
      if (!temp.properties[key]) {
        delete temp.properties[key]
      }
    }
  })

  return temp
}
