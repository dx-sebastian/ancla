export interface DevProfile {
    key: string;
    label: string;
    ports: readonly number[];
}

export interface DevFilter {
    key: string;
    labelEn: string;
    labelEs: string;
    profileKey: string | null;
}

export const ALL_FILTER_KEY = 'all';
export const DEV_ONLY_FILTER_KEY = 'dev-only';

const NONE: DevProfile = { key: '', label: '', ports: [] };
const DOCKER: DevProfile = { key: 'docker', label: 'Docker', ports: [2375, 2376, 2377, 4789, 7946] };

const PROFILES: readonly DevProfile[] = [
    { key: 'react',      label: 'React',      ports: [3000, 3001] },
    { key: 'sails',      label: 'Sails',      ports: Array.from({ length: 11 }, (_, i) => 1330 + i) },
    DOCKER,
    { key: 'vue',        label: 'Vue',        ports: [5173, 8080] },
    { key: 'angular',    label: 'Angular',    ports: [4200] },
    { key: 'ant-design', label: 'Ant Design', ports: [8000] },
    { key: 'storybook',  label: 'Storybook',  ports: [6006] },
    { key: 'aspnet',     label: 'ASP.NET',    ports: [5000, 5001] },
];

export class DevPortCatalog {
    getFilters(): DevFilter[] {
        return [
            { key: ALL_FILTER_KEY,     labelEn: 'All ports',    labelEs: 'Todos',        profileKey: null },
            { key: DEV_ONLY_FILTER_KEY, labelEn: 'Software dev', labelEs: 'Puertos dev',  profileKey: null },
            ...PROFILES.map(p => ({
                key: p.key,
                labelEn: p.label,
                labelEs: p.label,
                profileKey: p.key
            }))
        ];
    }

    match(port: number, processName: string, executablePath: string): DevProfile {
        if (this.looksLikeDocker(processName, executablePath)) {
            return DOCKER;
        }
        return PROFILES.find(p => (p.ports as number[]).includes(port)) ?? NONE;
    }

    private looksLikeDocker(processName: string, executablePath: string): boolean {
        return this.hasDockerToken(processName) || this.hasDockerToken(executablePath);
    }

    private hasDockerToken(value: string): boolean {
        if (!value) { return false; }
        const lower = value.toLowerCase();
        return lower.includes('docker') || lower.includes('com.docker');
    }
}
