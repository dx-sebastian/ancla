"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.DevPortCatalog = exports.DEV_ONLY_FILTER_KEY = exports.ALL_FILTER_KEY = void 0;
exports.ALL_FILTER_KEY = 'all';
exports.DEV_ONLY_FILTER_KEY = 'dev-only';
const NONE = { key: '', label: '', ports: [] };
const DOCKER = { key: 'docker', label: 'Docker', ports: [2375, 2376, 2377, 4789, 7946] };
const PROFILES = [
    { key: 'react', label: 'React', ports: [3000, 3001] },
    { key: 'sails', label: 'Sails', ports: Array.from({ length: 11 }, (_, i) => 1330 + i) },
    DOCKER,
    { key: 'vue', label: 'Vue', ports: [5173, 8080] },
    { key: 'angular', label: 'Angular', ports: [4200] },
    { key: 'ant-design', label: 'Ant Design', ports: [8000] },
    { key: 'storybook', label: 'Storybook', ports: [6006] },
    { key: 'aspnet', label: 'ASP.NET', ports: [5000, 5001] },
];
class DevPortCatalog {
    getFilters() {
        return [
            { key: exports.ALL_FILTER_KEY, labelEn: 'All ports', labelEs: 'Todos', profileKey: null },
            { key: exports.DEV_ONLY_FILTER_KEY, labelEn: 'Software dev', labelEs: 'Puertos dev', profileKey: null },
            ...PROFILES.map(p => ({
                key: p.key,
                labelEn: p.label,
                labelEs: p.label,
                profileKey: p.key
            }))
        ];
    }
    match(port, processName, executablePath) {
        if (this.looksLikeDocker(processName, executablePath)) {
            return DOCKER;
        }
        return PROFILES.find(p => p.ports.includes(port)) ?? NONE;
    }
    looksLikeDocker(processName, executablePath) {
        return this.hasDockerToken(processName) || this.hasDockerToken(executablePath);
    }
    hasDockerToken(value) {
        if (!value) {
            return false;
        }
        const lower = value.toLowerCase();
        return lower.includes('docker') || lower.includes('com.docker');
    }
}
exports.DevPortCatalog = DevPortCatalog;
//# sourceMappingURL=devPortCatalog.js.map