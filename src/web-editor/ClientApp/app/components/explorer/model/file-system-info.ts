
export class FileSystemInfo {
    name: string;
    path: PathInfo;
    isFile: boolean;

    constructor(path: PathInfo, isFile?: boolean) {
        if (!path) throw '[path] param should be defined.';

        this.path = path;
        this.isFile = isFile || false;
        this.name = path.parts[path.parts.length - 1];
    }

    get parent(): FileSystemInfo {
        if (!this.path.hasParent) return null;

        return new FileSystemInfo(this.path.parentPath, false);
    }

    toString() {
        return `<FileSystemInfo>[isFile:${this.isFile};path:${this.path}]`;
    }
}

export class PathInfo {
    static readonly pathSeparator = '/';
    parts: string[];

    constructor(pathParts: string[]) {
        if (!pathParts || pathParts.length === 0) throw new Error('Path have to contains at least one element');

        this.parts = pathParts;
    }
    toString(): string {
        return this.parts.join(PathInfo.pathSeparator);
    }

    concat(path: string) {
        return new PathInfo(this.parts.concat(path));
    }

    get hasParent() {
        return this.parts.length > 1;
    }

    get parentPath(): PathInfo {
        return new PathInfo(this.parts.slice(0, this.parts.length - 1))
    }

    static get default(): PathInfo {
        return new PathInfo(['.']);
    }

    static fromString(value: string) {
        if (!value) throw new Error('Path string can\'t be empty');
        const parts: string[] = value.split(PathInfo.pathSeparator);

        return new PathInfo(parts);
    }
}

export class FileSystemEntryBuilder {
    private pathInfo: PathInfo = PathInfo.default;
    private fileName: string;

    build(): FileSystemInfo {
        if (!this.fileName)
            return new FileSystemInfo(this.pathInfo, false);

        const fullPath = new PathInfo(this.pathInfo.parts.concat(this.fileName));
        return new FileSystemInfo(fullPath, true);
    }

    setFileName(value: string) {
        this.fileName = value;
    }

    setPath(parts: string[]) {
        this.pathInfo = new PathInfo(parts);
    }
}