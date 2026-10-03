import OpenApi from '@readme/openapi-parser'
import yaml from 'js-yaml'
import fs from 'fs'

let doc = yaml.load(fs.readFileSync('../schemas/openapi.yml'))

let endpoints = ''
let $refs = await OpenApi.resolve(doc)

const api = await OpenApi.parse(doc)

function toTitle(str) {
  const title = str.replace(/([A-Z])/g, ' $1')
  return title.charAt(0).toUpperCase() + title.slice(1)
}
function generatePathAttributes(parameters) {
  if (!parameters) {
    return ''
  }
  const properties = parameters.map(
    (param) => `
    <Property name="${param.name}" type="${param.schema.type}">
        ${param.description || ''}
    </Property>`,
  )
  return `### Path attributes

  <Properties>
    ${properties.join('')}
  </Properties>`
}

function generateBodyAttributes(attributes) {
  if (!attributes) {
    return ''
  }
  const properties = Object.keys(attributes.properties).map(
    (key) => `
    <Property name="${key}" type="${
      attributes.properties[key].type || 'any'
    }" required="${attributes.required?.includes(key)}">
        ${attributes.properties[key].description || ''}
    </Property>`,
  )
  return `### Body attributes

  <Properties>
    ${properties.join('')}
  </Properties>`
}
function generateCurlFormData(properties) {
  if (!properties) {
    return ''
  }
  return Object.keys(properties)
    .map((key) => {
      const line =
        properties[key].format === 'binary'
          ? `-F '@${key}=[${key}-path]'`
          : `-F '${key}=[${key}]'`
      return '    ' + line
    })
    .join('\n')
}

function resolveRef(ref) {
  const temp = $refs.get(ref)
  if (!temp.properties) {
    return temp
  }
  Object.keys(temp.properties).forEach((key) => {
    if (temp.properties[key]['$ref']) {
      temp.properties[key] = resolveRef(temp.properties[key]['$ref'])
      if (!temp.properties[key]) {
        delete temp.properties[key]
      }
    }
  })
  return temp
}

function generateEndpoint(endpoint, raw_method, raw_path) {
  let method = raw_method.toUpperCase()
  const path = raw_path.replaceAll('{', '<').replaceAll('}', '>')
  const content = endpoint.requestBody?.content
  let attributes = undefined
  if (content) {
    const contentType = Object.keys(content)[0]
    attributes = content[contentType].schema['$ref']
      ? resolveRef(content[contentType].schema['$ref'])
      : content[contentType].schema
  }

  return `
## ${toTitle(endpoint.operationId)} {{ tag: '${method}', label: '${path}' }}
<Row>
  <Col>
    ${endpoint.summary}

    ${generatePathAttributes(endpoint.parameters)}
    ${generateBodyAttributes(attributes)}
   
    ### Response
  
    ${endpoint.responses['200'].description}

  </Col>
  <Col sticky>
    <CodeGroup title="Request" tag="${method}" label="${path}">
\`\`\`bash {{ title: 'cURL' }}
curl ${method === 'POST' ? '--request POST' : ''} http://localhost:8000${path
    .replaceAll('<', '[')
    .replaceAll('>', ']')}
${generateCurlFormData(attributes?.properties)}
\`\`\`
    </CodeGroup>
  </Col>
</Row>

---
`
}

Object.keys(api.paths).forEach((path) => {
  Object.keys(api.paths[path]).forEach((method) => {
    let endpoint = api.paths[path][method]
    endpoints += generateEndpoint(endpoint, method, path)
  })
})

let template = fs.readFileSync('./src/app/endpoints/page.template.mdx', 'utf8')
fs.writeFileSync('./src/app/endpoints/page.mdx', template + endpoints)
