{{/*
Expand the name of the chart.
*/}}
{{- define "dawning.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
*/}}
{{- define "dawning.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "dawning.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "dawning.labels" -}}
helm.sh/chart: {{ include "dawning.chart" . }}
{{ include "dawning.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "dawning.selectorLabels" -}}
app.kubernetes.io/name: {{ include "dawning.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Identity API labels
*/}}
{{- define "dawning.identityApi.labels" -}}
{{ include "dawning.labels" . }}
app.kubernetes.io/component: identity-api
{{- end }}

{{- define "dawning.identityApi.selectorLabels" -}}
{{ include "dawning.selectorLabels" . }}
app.kubernetes.io/component: identity-api
{{- end }}

{{/*
Gateway API labels
*/}}
{{- define "dawning.gatewayApi.labels" -}}
{{ include "dawning.labels" . }}
app.kubernetes.io/component: gateway-api
{{- end }}

{{- define "dawning.gatewayApi.selectorLabels" -}}
{{ include "dawning.selectorLabels" . }}
app.kubernetes.io/component: gateway-api
{{- end }}

{{/*
Admin Frontend labels
*/}}
{{- define "dawning.adminFrontend.labels" -}}
{{ include "dawning.labels" . }}
app.kubernetes.io/component: admin-frontend
{{- end }}

{{- define "dawning.adminFrontend.selectorLabels" -}}
{{ include "dawning.selectorLabels" . }}
app.kubernetes.io/component: admin-frontend
{{- end }}

{{/*
Database connection string
*/}}
{{- define "dawning.databaseHost" -}}
{{- if .Values.database.external.enabled }}
{{- .Values.database.external.host }}
{{- else }}
{{- printf "%s-mysql" (include "dawning.fullname" .) }}
{{- end }}
{{- end }}

{{- define "dawning.databasePort" -}}
{{- if .Values.database.external.enabled }}
{{- .Values.database.external.port }}
{{- else }}
{{- 3306 }}
{{- end }}
{{- end }}

{{- define "dawning.databaseName" -}}
{{- if .Values.database.external.enabled }}
{{- .Values.database.external.database }}
{{- else }}
{{- .Values.mysql.auth.database }}
{{- end }}
{{- end }}

{{- define "dawning.databaseUsername" -}}
{{- if .Values.database.external.enabled }}
{{- .Values.database.external.username }}
{{- else }}
{{- .Values.mysql.auth.username }}
{{- end }}
{{- end }}

{{/*
Redis connection
*/}}
{{- define "dawning.redisHost" -}}
{{- if .Values.cache.external.enabled }}
{{- .Values.cache.external.host }}
{{- else }}
{{- printf "%s-redis" (include "dawning.fullname" .) }}
{{- end }}
{{- end }}

{{- define "dawning.redisPort" -}}
{{- if .Values.cache.external.enabled }}
{{- .Values.cache.external.port }}
{{- else }}
{{- 6379 }}
{{- end }}
{{- end }}

{{/*
Kafka connection
*/}}
{{- define "dawning.kafkaBootstrapServers" -}}
{{- if .Values.messaging.external.enabled }}
{{- .Values.messaging.external.bootstrapServers }}
{{- else }}
{{- printf "%s-kafka:9092" (include "dawning.fullname" .) }}
{{- end }}
{{- end }}

{{/*
Generate random string for secrets
*/}}
{{- define "dawning.randomSecret" -}}
{{- randAlphaNum 32 | b64enc }}
{{- end }}
